using System;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

namespace InnerService
{
    /// <summary>
    /// Вспомогательный класс для работы с БД
    /// </summary>
    class DatabaseHelper
    {
        // SQL запрос на выгрузку аудиторий, участвующих в видеонаблюдении
        private static string Query = @"
            WITH er (ExamDate, RegionCode, RegionName)
              AS (SELECT DISTINCT e.ExamDate, r.REGION, r.RegionName
                    FROM dat_Exams AS e
                   CROSS JOIN rbdc_Regions AS r
                   WHERE EXISTS(SELECT peos.ParticipantsExamsOnStationID
                                  FROM dbo.rbd_ParticipantsExamsOnStation AS peos
                                 WHERE r.REGION = peos.Region AND
                                       e.ExamGlobalID = peos.ExamGlobalID)),
              de (ExamGlobalID, SubjectCode, SubjectName)
              AS (SELECT e.ExamGlobalID, s.SubjectCode, s.SubjectName
                    FROM dat_Exams AS e
                    JOIN dat_Subjects AS s
                      ON e.SubjectCode = s.SubjectCode),
              se (REGION, StationID, StationCode, StationName, SchoolFK, ExamDate)
              AS (SELECT DISTINCT rse.REGION, rse.StationID, stt.StationCode, stt.StationName,
                         stt.SchoolFK, de.ExamDate
                    FROM rbd_StationsExams AS rse
                    JOIN rbd_Stations AS stt
                      ON stt.StationID = rse.StationID
                    JOIN dat_Exams AS de
                      ON de.ExamGlobalID = rse.ExamGlobalID
                   WHERE ISNULL(rse.IsDeleted, 0) = 0 AND
                         stt.DeleteType = 0),
                  au(REGION, StationID, ExamDate, AuditoriumID, AuditoriumName, AuditoriumCode, LimitPotencial, SeatsCount)
              AS (SELECT DISTINCT sea.REGION, sea.StationID, de.ExamDate, sea.AuditoriumID, a.AuditoriumName, a.AuditoriumCode, a.LimitPotencial,
                         a.RowsCount * a.ColsCount AS SeatsCount
                    FROM rbd_StationExamAuditory AS sea
                    JOIN rbd_StationsExams AS se
                      ON se.StationsExamsID = sea.StationsExamsID
                    JOIN rbd_Auditoriums AS a
                      ON a.REGION = sea.Region AND
                         a.StationID = sea.StationID AND
                         a.AuditoriumID = sea.AuditoriumID
                    JOIN dat_Exams AS de
                      ON de.ExamGlobalID = se.ExamGlobalID
                   WHERE a.DeleteType = 0),
                  ae(REGION, StationID, AuditoriumID, AuditoriumName, AuditoriumCode, LimitPotencial, SeatsCount,
                     ExamGlobalID, SubjectCode, SubjectName, ExamDate,
                     WithDisablilities, SpecialParticipants, ParticipantsTotal)
              AS (SELECT a.REGION, se.StationID, a.AuditoriumID,
                         a.AuditoriumName, a.AuditoriumCode, a.LimitPotencial, a.RowsCount * a.ColsCount AS SeatsCount,
                         de.ExamGlobalID, ds.SubjectCode, ds.SubjectName, de.ExamDate,
                         SUM(CASE WHEN pp.PropertyId IS NOT NULL AND p.LimitPotencial = 1 THEN 1 ELSE 0 END) AS WithDisablilities,
                         SUM(CASE WHEN peps.PExamPlacesOnStationID IS NULL THEN 0
                                  WHEN (peps.IsManual = 1) OR (pp.PropertyId IS NOT NULL AND p.LimitPotencial = 1 AND a.LimitPotencial = 1)
                                  THEN 1 ELSE 0 END) AS SpecialParticipants,
                         COUNT(DISTINCT p.ParticipantID) AS ParticipantsTotal
                    FROM dbo.rbd_Auditoriums AS a
                    JOIN dbo.rbd_StationExamAuditory AS ea
                      ON ea.AuditoriumID = a.AuditoriumID AND
                         ea.Region = a.REGION
                    JOIN dbo.rbd_StationsExams AS se
                      ON se.StationsExamsID = ea.StationsExamsID
                    JOIN dat_Exams AS de
                      ON de.ExamGlobalID = se.ExamGlobalID
                    JOIN dat_Subjects AS ds
                      ON de.SubjectCode = ds.SubjectCode
                    LEFT JOIN dbo.orlv_ParticipantsExamPStation AS peps
                      ON a.AuditoriumID = peps.AuditoriumID AND
                         peps.StationsExamsID = se.StationsExamsID AND
                         ISNULL(peps.IsDeleted, 0) = 0
                    LEFT JOIN dbo.rbd_ParticipantsExamsOnStation AS peos
                      ON peos.ParticipantsExamsOnStationID = peps.ParticipantsExamsOnStationID AND
                         ISNULL(peos.IsDeleted, 0) = 0
                    LEFT JOIN rbd_ParticipantsExams AS pe
                      ON pe.ParticipantsExamsID = peos.ParticipantsExamsID AND
                         ISNULL(pe.IsDeleted, 0) = 0
                    LEFT JOIN dbo.rbd_Participants AS p
                      ON p.ParticipantID = pe.ParticipantID AND
                         p.DeleteType = 0
                    LEFT JOIN dbo.rbd_ParticipantProperties AS pp
                      ON p.ParticipantID = pp.ParticipantID AND
                         ISNULL(pp.IsDeleted, 0) = 0 AND
                         pp.Property = 7 AND
                         pp.PValue = 1
                   WHERE a.DeleteType = 0 AND
                         ISNULL(ea.IsDeleted, 0) = 0 AND
                         ISNULL(se.IsDeleted, 0) = 0
                   GROUP BY a.REGION, se.StationID, a.AuditoriumID,
                            a.AuditoriumName, a.AuditoriumCode, a.LimitPotencial, a.RowsCount * a.ColsCount,
                            de.ExamGlobalID, ds.SubjectCode, ds.SubjectName, de.ExamDate)
            SELECT e.ExamDate AS '@date', 'plan' AS '@type', 
                   r.regions
              FROM (SELECT DISTINCT ExamDate, TestTypeCode FROM dat_Exams) AS e
             CROSS APPLY
                   (SELECT er.RegionCode AS '@code', er.RegionName AS '@name', st.stations
                      FROM er
                     CROSS APPLY
                           (SELECT se.StationID AS '@id', se.StationCode AS '@code', se.StationName AS '@name',
                                   se.SchoolFK AS '@school_id', a.auditoriums
                              FROM se
                             CROSS APPLY
                                   (SELECT au.AuditoriumID AS '@id', au.AuditoriumName AS '@name',
                                           au.AuditoriumCode AS '@code', au.LimitPotencial AS '@can_be_special',
                                           au.SeatsCount AS '@seats', s.subjects
                                      FROM au
                                     CROSS APPLY
                                           (SELECT ae.SubjectName AS '@name',
                                                   ae.WithDisablilities AS '@with_disabilities',
                                                   ae.LimitPotencial AS '@special',
                                                   ae.ParticipantsTotal AS '@total'
                                              FROM ae
                                             WHERE ae.AuditoriumID = au.AuditoriumID AND
                                                   ae.ExamDate = e.ExamDate
                                             ORDER BY ae.SubjectCode
                                               FOR XML PATH('subject'), TYPE) AS s(subjects)
                                      WHERE au.StationID = se.StationID AND
                                            au.ExamDate = e.ExamDate
                                      ORDER BY au.AuditoriumCode
                                    FOR XML PATH('auditorium'), TYPE) AS a(auditoriums)
                             WHERE se.ExamDate = er.ExamDate AND er.RegionCode = se.REGION
                             FOR XML PATH('station'), TYPE) AS st(stations)
                          WHERE er.ExamDate = e.ExamDate
                          ORDER BY er.RegionCode
                            FOR XML PATH('region'), TYPE) AS r(regions)
             WHERE e.ExamDate = ? and e.TestTypeCode=4
                FOR XML PATH('schedule')
            ";

        /// <summary>
        /// Метод, выгружающий данные в формате XML во временный каталог в файл
        /// </summary>
        public static void WriteXML()
        {
            // замыкание инициализации объекта соединения с базой данных
            using (SqlConnection connection = new SqlConnection(Globals.GetConnectionString()))
            {
                // открытие соединения с базой данных
                connection.Open();

                // команда запроса
                SqlCommand command = new SqlCommand(Query, connection);
                // настройки сериализации в XML
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                    CloseOutput = true
                };
                // замыкание объекта чтения как XML сериализованных данных
                using (XmlReader reader = command.ExecuteXmlReader())
                // замыкание объекта записи XML данных
                using (XmlWriter writer = XmlWriter.Create(Globals.GetTempFilePath(), settings))
                {
                    // запись XML данных как цельной XML записи
                    writer.WriteNode(reader, true);
                }
            }
        }
        /// <summary>
        /// Проверить соединение с БД
        /// </summary>
        /// <returns></returns>
        public static bool CheckConnection()
        {
            // переменная результата
            bool result = false;
            try
            {
                // соединение
                using (var connection = new SqlConnection(Globals.GetConnectionString()))
                {
                    // фиктивный запрос
                    var query = "select 1";
                    var command = new SqlCommand(query, connection);
                    connection.Open();
                    command.ExecuteScalar();
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

    }
}
