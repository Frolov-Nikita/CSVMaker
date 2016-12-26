using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="Range">Добавляемые</param>
        public static void AddRange<T>(this Collection<T> col, IEnumerable<T> Range)
        {
            foreach (var newItem in Range)
                col.Add(newItem);
        }

        /// <summary>
        /// Удаляет из коллекции диапазон
        /// </summary>
        /// <typeparam name="T">тип коллекции</typeparam>
        /// <param name="col">Коллекция</param>
        /// <param name="Range">удаляемые элементы</param>
        public static void RemoveRange<T>(this Collection<T> col, IEnumerable<T> Range)
        {
            foreach (var oldItem in Range)
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
