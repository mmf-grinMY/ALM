using System.Text.RegularExpressions;

using Autodesk.AutoCAD.Geometry;

namespace ALM.Entities.Wkt
{
    /// <summary>
    /// Парсер формата Wkt
    /// </summary>
    static class Parser
    {
        /// <summary>
        /// Регулярное выражение для точки
        /// </summary>
        readonly static Regex point;
        /// <summary>
        /// Инициализация парсера
        /// </summary>
        static Parser()
        {
            point = new Regex(@"\d+(\.\d{0,3})? \d+(\.\d{0,3})?");
        }
        /// <summary>
        /// Парсить точку
        /// </summary>
        /// <param name="wkt">Исходная строка в формате Wkt</param>
        /// <returns>Полученная точка</returns>
        public static Point3d ParsePoint(string wkt)
        {
            var coords = point.Match(wkt).Value.Split(' ');

            return new Point3d(coords[0].ToDouble(), coords[1].ToDouble(), 0);
        }
        /// <summary>
        /// Конвертация строки в вещественное число
        /// </summary>
        /// <param name="str">Строковое представление числа</param>
        /// <returns>Вещественное число</returns>
        public static double ToDouble(this string str)
        {
            if (str is null)
                return 0.0;

            if (str.Contains("_"))
                str = str.Replace('_', '.');
            else if (str.Contains(","))
                str = str.Replace(',', '.');

            return double.Parse(str, System.Globalization.CultureInfo.GetCultureInfo("en"));
        }
    }
}
