﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataGrid.cs" company="2GIS">
//   Cruciatus
// </copyright>
// <summary>
//   Представляет элемент управления таблица.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruciatus.Elements
{
    using System;
    using System.Windows.Automation;

    using Cruciatus.Exceptions;
    using Cruciatus.Extensions;
    using Cruciatus.Interfaces;

    /// <summary>
    /// Представляет элемент управления таблица.
    /// </summary>
    public class DataGrid : CruciatusElement, IContainerElement
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataGrid"/>.
        /// </summary>
        public DataGrid()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataGrid"/>.
        /// </summary>
        /// <param name="parent">
        /// Элемент, являющийся родителем для таблицы.
        /// </param>
        /// <param name="automationId">
        /// Уникальный идентификатор таблицы.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Входные параметры не должны быть нулевыми.
        /// </exception>
        public DataGrid(AutomationElement parent, string automationId)
        {
            Initialize(parent, automationId);
        }

        /// <summary>
        /// Возвращает значение, указывающее, включена ли таблица.
        /// </summary>
        /// <exception cref="PropertyNotSupportedException">
        /// Таблица не поддерживает данное свойство.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// При получении значения свойства не удалось привести его к ожидаемому типу.
        /// </exception>
        public bool IsEnabled
        {
            get
            {
                return this.GetPropertyValue<bool>(AutomationElement.IsEnabledProperty);
            }
        }

        /// <summary>
        /// Возвращает количество строк в таблице.
        /// </summary>
        /// <exception cref="PropertyNotSupportedException">
        /// Таблица не поддерживает данное свойство.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// При получении значения свойства не удалось привести его к ожидаемому типу.
        /// </exception>
        public int RowCount
        {
            get
            {
                return this.GetPropertyValue<int>(GridPattern.RowCountProperty);
            }
        }

        /// <summary>
        /// Возвращает количество столбцов в таблице.
        /// </summary>
        /// <exception cref="PropertyNotSupportedException">
        /// Таблица не поддерживает данное свойство.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// При получении значения свойства не удалось привести его к ожидаемому типу.
        /// </exception>
        public int ColumnCount
        {
            get
            {
                return this.GetPropertyValue<int>(GridPattern.ColumnCountProperty);
            }
        }

        /// <summary>
        /// Возвращает текстовое представление имени класса.
        /// </summary>
        internal override string ClassName
        {
            get
            {
                return "DataGrid";
            }
        }

        internal override ControlType GetType
        {
            get
            {
                return ControlType.DataGrid;
            }
        }

        /// <summary>
        /// Выполняет прокрутку до ячейки с указанным номером строки и колонки.
        /// </summary>
        /// <param name="row">
        /// Номер строки.
        /// </param>
        /// <param name="column">
        /// Номер колонки.
        /// </param>
        /// <returns>
        /// Значение true если прокрутить удалось либо в этом нет необходимости;
        /// в противном случае значение - false.
        /// </returns>
        public bool ScrollTo(int row, int column)
        {
            // Проверка на дурака
            if (row < 0 || column < 0)
            {
                this.LastErrorMessage = string.Format(
                    "В {0} ячейка [{1}, {2}] не существует, т.к. задан отрицательный номер.",
                    this.ToString(),
                    row,
                    column);
                return false;
            }

            // Получение шаблона прокрутки у таблицы
            var scrollPattern = this.Element.GetCurrentPattern(ScrollPattern.Pattern) as ScrollPattern;
            if (scrollPattern == null)
            {
                this.LastErrorMessage = string.Format("{0} не поддерживает шаблон прокрутки.", this.ToString());
                return false;
            }

            // Условие для вертикального поиска ячейки [row, 0] (через строку)
            var cellCondition = new AndCondition(
                new PropertyCondition(AutomationElement.IsGridItemPatternAvailableProperty, true),
                new PropertyCondition(GridItemPattern.RowProperty, row));

            // Стартовый поиск ячейки
            var cell = this.Element.FindFirst(TreeScope.Subtree, cellCondition);

            // Основная вертикальная прокрутка (при необходимости и возможности)
            if (cell == null && scrollPattern.Current.VerticallyScrollable)
            {
                while (cell == null && scrollPattern.Current.VerticalScrollPercent < 99.9)
                {
                    scrollPattern.ScrollVertical(ScrollAmount.LargeIncrement);
                    cell = this.Element.FindFirst(TreeScope.Subtree, cellCondition);
                }
            }

            // Если прокрутив до конца ячейка не найдена, то номер строки не действительный
            if (cell == null)
            {
                this.LastErrorMessage = string.Format("В {0} нет строки с номером {1}.", this.ToString(), row);
                return false;
            }

            // Докручиваем по вертикали, пока ячейку [row, 0] не станет видно
            while (!this.Element.GeometricallyContains(cell))
            {
                cell = this.Element.FindFirst(TreeScope.Subtree, cellCondition);
                scrollPattern.ScrollVertical(ScrollAmount.SmallIncrement);
            }

            // Условие для горизонтального поиска ячейки [row, column]
            cellCondition = new AndCondition(
                new PropertyCondition(AutomationElement.IsGridItemPatternAvailableProperty, true),
                new PropertyCondition(GridItemPattern.RowProperty, row),
                new PropertyCondition(GridItemPattern.ColumnProperty, column));

            // Стартовый поиск ячейки
            cell = this.Element.FindFirst(TreeScope.Subtree, cellCondition);

            // Основная горизонтальная прокрутка (при необходимости и возможности)
            if (cell == null && scrollPattern.Current.HorizontallyScrollable)
            {
                while (cell == null && scrollPattern.Current.HorizontalScrollPercent < 99.9)
                {
                    scrollPattern.ScrollHorizontal(ScrollAmount.LargeIncrement);
                    cell = this.Element.FindFirst(TreeScope.Subtree, cellCondition);
                }
            }

            // Если прокрутив до конца ячейка не найдена, то номер колонки не действительный
            if (cell == null)
            {
                this.LastErrorMessage = string.Format("В {0} нет колонки с номером {1}.", this.ToString(), column);
                return false;
            }

            // Докручиваем по горизонтали, пока ячейку [row, column] не станет видно
            while (!this.Element.GeometricallyContains(cell))
            {
                cell = this.Element.FindFirst(TreeScope.Subtree, cellCondition);
                scrollPattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
            }

            return true;
        }

        /// <summary>
        /// Возвращает элемент заданного типа с указанным номером строки и колонки.
        /// </summary>
        /// <param name="row">
        /// Номер строки.
        /// </param>
        /// <param name="column">
        /// Номер колонки.
        /// </param>
        /// <typeparam name="T">
        /// Тип элемента.
        /// </typeparam>
        /// <returns>
        /// Искомый элемент, либо null, если найти не удалось.
        /// </returns>
        public T Item<T>(int row, int column) where T : CruciatusElement, IListElement, new()
        {
            // Проверка, что таблица включена
            var isEnabled = CruciatusFactory.WaitingValues(
                    () => this.IsEnabled,
                    value => value != true);
            if (!isEnabled)
            {
                this.LastErrorMessage = string.Format("{0} отключена.", this.ToString());
                return null;
            }

            // Проверка на дурака
            if (row < 0 || column < 0)
            {
                this.LastErrorMessage = string.Format(
                    "В {0} ячейка [{1}, {2}] не существует, т.к. задан отрицательный номер.",
                    this.ToString(),
                    row,
                    column);
                return null;
            }

            // Условие для поиска ячейки [row, column]
            var cellCondition = new AndCondition(
                new PropertyCondition(AutomationElement.IsGridItemPatternAvailableProperty, true),
                new PropertyCondition(GridItemPattern.RowProperty, row),
                new PropertyCondition(GridItemPattern.ColumnProperty, column));
            var cell = this.Element.FindFirst(TreeScope.Subtree, cellCondition);

            // Проверка, что ячейку видно
            if (cell == null || !this.Element.GeometricallyContains(cell))
            {
                this.LastErrorMessage = string.Format(
                    "В {0} ячейка [{1}, {2}] вне видимости или не существует.",
                    this.ToString(),
                    row,
                    column);
                return null;
            }

            // Поиск подходящего элемента в ячейке
            var item = new T();
            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, item.GetType);
            var elem = cell.FindFirst(TreeScope.Subtree, condition);
            if (elem == null)
            {
                this.LastErrorMessage = string.Format(
                    "В {0}, ячейка [{1}, {2}], нет элемента желаемого типа.",
                    this.ToString(),
                    row,
                    column);
                return null;
            }

            item.Initialize(elem);
            return item;
        }

        void IContainerElement.Initialize(AutomationElement parent, string automationId)
        {
            Initialize(parent, automationId);
        }
    }
}