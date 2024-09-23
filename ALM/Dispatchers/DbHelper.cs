using ALM.Entities;
using ALM.View;

using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

using Autodesk.AutoCAD.Geometry;
using ALM.Logging;
using System.Text.RegularExpressions;

namespace ALM
{
    /// <summary>
    /// Класс-помощник для получения данных из БД
    /// </summary>
    static class DbHelper
    {
        /// <summary>
        /// Получение данных от пользователя
        /// </summary>
        /// <typeparam name="TWindow">Окно ввода</typeparam>
        /// <param name="window">Окно работы с пользователем</param>
        /// <returns>Введенные пользователем данные</returns>
        static object[] GetResult<TWindow>(TWindow window) where TWindow : Window, IResult
        {
            object result = null;
            bool isCanceled = true;

            window.ShowDialog();

            if (window.IsSuccess)
            {
                result = window.Result;
                isCanceled = false;
            }

            window.Close();

            return new object[] { result, isCanceled };
        }
        /// <summary>
        /// Получение строки подключения к БД
        /// </summary>
        public static object[] ConnectionStr => GetResult(new LoginWindow());
        /// <summary>
        /// Получение строки подключения без учета BoundingBox
        /// </summary>
        public static string SimpleConnectionStr => (GetResult(new LoginWindow(false))[0] as object[])[0].ToString();
        /// <summary>
        /// Получение рисуемого горизонта
        /// </summary>
        /// <param name="horizons">Список доступных для отрисовки горизонтов</param>
        /// <returns>Выбранный горизонт</returns>
        public static object[] SelectHorizon(ObservableCollection<string> horizons) => 
            GetResult(new HorizonSelecterWindow(horizons));
        /// <summary>
        /// Создание команды выборки данных
        /// </summary>
        /// <param name="baseName">Имя линкованной таблицы</param>
        /// <param name="linkField">Столбец линковки</param>
        /// <param name="systemId">Уникальный номер примитива</param>
        /// <param name="fieldNames">Список столбцов таблицы</param>
        /// <returns>Команда для получения данных</returns>
        public static string CreateCommand(string baseName, string linkField, int systemId, IDictionary<string, string> fieldNames)
        {
            var builder = new StringBuilder().Append("SELECT ");

            foreach (var item in fieldNames)
            {
                builder.Append(item.Key).Append(" as \"").Append(item.Value).Append("\"").Append(",");
            }

            builder
                .Remove(builder.Length - 1, 1)
                .Append(" FROM ")
                .Append(baseName)
                .Append(" WHERE ")
                .Append(linkField)
                .Append(" = ")
                .Append(systemId);

            return builder.ToString();
        }
        /// <summary>
        /// Получение списка столбцов таблицы
        /// </summary>
        /// <param name="fields">Исходные столбцы</param>
        /// <returns>Список столбцов</returns>
        public static Dictionary<string, string> ParseFieldNames(IEnumerable<string> fields)
        {
            bool fieldsFlag = true;
            var result = new Dictionary<string, string>();

            foreach (var field in fields)
            {
                if (fieldsFlag)
                {
                    if (field == "FIELDS")
                    {
                        fieldsFlag = false;
                    }
                    continue;
                }
                else if (field == "ENDFIELDS")
                {
                    break;
                }
                else if (field.Contains("+"))
                {
                    continue;
                }
                var rows = field.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (rows.Length <= 1) continue;

                var builder = new StringBuilder();

                for (int j = 1; j < rows.Length; ++j)
                {
                    builder.Append(rows[j]).Append("_");
                }

                if (!result.ContainsKey(rows[0]))
                {
                    result.Add(rows[0], builder.ToString());
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="primitive"></param>
        /// <returns></returns>
        /// <exception cref="ParseException"/>
        public static Autodesk.AutoCAD.DatabaseServices.Polyline[] Parse(IDbDispatcher dispatcher, Primitive primitive, ILogger logger)
        {
            return _Parse(dispatcher, primitive, logger).Map();
        }
#pragma warning disable IDE1006 // Стили именования
        internal static Geometry _Parse(IDbDispatcher dispatcher, Primitive primitive, ILogger logger)
#pragma warning restore IDE1006 // Стили именования
        {
            var reader = new WKTReader();

            var wkt = primitive.Geometry;
            try
            {
                return reader.Read(wkt);
            }
            catch (ParseException)
            {
                wkt = dispatcher.GetLongGeometry(primitive);
                try
                {
                    return reader.Read(wkt);
                }
                catch (ParseException)
                {
                    try
                    {
                        return reader.Read(Regex.Replace(wkt, "\\(\\),?", string.Empty));
                    }
                    catch (ParseException)
                    {
                        logger.LogError("Не удалось получить геомерию объекта {0}", primitive.Guid);
                        return new LineString(Array.Empty<Coordinate>());
                    }
                }
            }
            catch (ArgumentException)
            {
                if (new Regex("[MULTILINESTRING|POLYGON]\\(\\(\\d+(\\.\\d{0,3})? \\d+(\\.\\d{0,3})?\\)\\)").IsMatch(wkt))
                {
                    return new LineString(Array.Empty<Coordinate>());
                }
                else
                {
                    logger.LogError("Не удалось получить геомерию объекта {0}", primitive.Guid);
                    return new LineString(Array.Empty<Coordinate>());
                }
            }
        }
    }
    public static class MapperExtenions
    {
        public static Autodesk.AutoCAD.DatabaseServices.Polyline[] Map(this Geometry geometry)
        {
            var polylines = new Autodesk.AutoCAD.DatabaseServices.Polyline[geometry.NumGeometries];

            for (int i = 0; i < polylines.Length; i++)
            {
                polylines[i] = new Autodesk.AutoCAD.DatabaseServices.Polyline();
                var linestring = geometry.GetGeometryN(i);
                for (int j = 0; j < linestring.Coordinates.Length; j++)
                {
                    polylines[i].AddVertexAt(j, linestring.Coordinates[j].Map(), 0, 0, 0);
                }
            }

            return polylines;
        }
        public static Point2d Map(this Coordinate coordinate) => new Point2d(coordinate.X, coordinate.Y);
    }
}