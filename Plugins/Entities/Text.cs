﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Newtonsoft.Json.Linq;

namespace Plugins.Entities
{
    /// <summary>
    /// Подпись
    /// </summary>
    sealed class Text : StyledEntity
    {
        #region Ctor

        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="primitive">Параметры отрисовки</param>
        /// <param name="logger">Логер событий</param>
        /// <param name="style">Стиль отрисовки</param>
        public Text(Primitive primitive, MyEntityStyle style) : base(primitive, style) { }

        #endregion

        #region Protected Methods

        protected override void Draw(Transaction transaction, BlockTable table, BlockTableRecord record, Logging.ILogger logger)
        {
            base.Draw(transaction, table, record, logger);

            const string FONT_SIZE = "FontSize";
            const string ANGLE = "Angle";
            const string TEXT = "Text";
            const string X_OFFSET = "XOffset";
            const string Y_OFFSET = "YOffset";

            var settings = primitive.DrawSettings;
            var fontSize = settings.Value<int>(FONT_SIZE) * style.scale;
            var paramJson = primitive.Param;
            var offset = new Vector3d(paramJson.Value<string>(X_OFFSET).ToDouble(), paramJson.Value<string>(Y_OFFSET).ToDouble(), 0);

            // FIXME: Присваивать высоту 10 для объектов с высотой 0
            try
            {
                var text = new DBText()
                {
                    Layer = primitive.LayerName,
                    Color = Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0),
                    Position = Wkt.Parser.ParsePoint(primitive.Geometry) + offset,
                    TextString = settings.Value<string>(TEXT),
                    Height = fontSize,
                };

                if (primitive.Param.TryGetValue(ANGLE, System.StringComparison.CurrentCulture, out JToken angle))
                {
                    text.Rotation = angle.Value<string>().ToDouble().ToRad();
                }

                text.AppendToDb(transaction, record, primitive);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                if (!e.StackTrace.Contains("DBText.set_Height"))
                    throw;
            }
        }

        #endregion
    }
}