using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace CSVMaker.Model
{
    /// <summary>
    /// Различные расширения
    /// </summary>
    public static class MiscExtentions
    {
        /// <summary>
        /// Добавит в коллекцию диапазон значений
        /// </summary>
        /// <typeparam name="T">тип коллекции</typeparam>
        /// <param name="col">Коллекция</param>
        /// <param name="range">Добавляемые</param>
        public static void AddRange<T>(this Collection<T> col, IEnumerable<T> range)
        {
            foreach (var newItem in range)
                col.Add(newItem);
        }

        /// <summary>
        /// Удаляет из коллекции диапазон
        /// </summary>
        /// <typeparam name="T">тип коллекции</typeparam>
        /// <param name="col">Коллекция</param>
        /// <param name="range">удаляемые элементы</param>
        public static void RemoveRange<T>(this Collection<T> col, IEnumerable<T> range)
        {
            foreach (var oldItem in range)
                if(col.Contains(oldItem))
                    col.Remove(oldItem);
        }

        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
                where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }
    }//MiscExtentions
}
