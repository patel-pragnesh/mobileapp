using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.Collections
{
    public struct SectionedIndex
    {
        public int Section;
        public int Row;
    }

    public class GroupedOrderedList<TItem>
    {
        private List<List<TItem>> sections;
        private Func<TItem, IComparable> orderingKey;
        private Func<TItem, IComparable> groupingKey;
        private bool descending;

        public IReadOnlyList<IReadOnlyList<TItem>> Items
            => sections;

        public GroupedOrderedList(Func<TItem, IComparable> orderingKey, Func<TItem, IComparable> groupingKey, IList<TItem> initialItems = null, bool descending = false)
        {
            this.orderingKey = orderingKey;
            this.groupingKey = groupingKey;
            this.descending = descending;

            if (initialItems == null)
            {
                sections = new List<List<TItem>> { };
            }
            else
            {
                sections = initialItems
                    .OrderBy(orderingKey)
                    .GroupBy(groupingKey)
                    .Select(g => g.ToList())
                    .ToList();
            }
        }

        public TItem ItemAt(int section, int row)
        {
            return sections[section][row];
        }

        public virtual SectionedIndex? RemoveItem(TItem item)
        {
            var sectionIndex = sections
                .IndexOf(g => groupingKey(g.First()).CompareTo(groupingKey(item)) == 0);

            if (sectionIndex == -1)
                return null;

            var rowIndex = sections[sectionIndex].IndexOf(item);
            if (rowIndex == -1)
                return null;

            sections[sectionIndex].RemoveAt(rowIndex);

            if (sections[sectionIndex].Count == 0)
                sections.RemoveAt(sectionIndex);

            return new SectionedIndex { Section = sectionIndex, Row = rowIndex };
        }

        public virtual TItem RemoveItemAt(int section, int row)
        {
            var item = sections[section][row];
            sections[section].RemoveAt(row);
            return item;
        }

        public virtual SectionedIndex InsertItem(TItem item)
        {
            var sectionIndex = sections
                .IndexOf(g => groupingKey(g.First()).CompareTo(groupingKey(item)) == 0);

            if (sectionIndex == -1)
            {
                var insertionIndex = sections.FindLastIndex(g => groupingKey(g.First()).CompareTo(groupingKey(item)) < 0);
                List<TItem> list = new List<TItem> { item };
                if (insertionIndex == -1)
                {
                    sections.Insert(0, list);
                    return new SectionedIndex { Section = 0, Row = 0 };
                }
                else
                {
                    sections.Insert(insertionIndex + 1, list);
                    return new SectionedIndex { Section = insertionIndex + 1, Row = 0 };
                }
            }
            else
            {
                var rowIndex = sections[sectionIndex].FindLastIndex(i => orderingKey(i).CompareTo(orderingKey(item)) < 0);
                if (rowIndex == -1)
                {
                    sections[sectionIndex].Insert(0, item);
                    return new SectionedIndex { Section = sectionIndex, Row = 0 };
                }
                else
                {
                    sections[sectionIndex].Insert(rowIndex + 1, item);
                    return new SectionedIndex { Section = sectionIndex, Row = rowIndex + 1 };
                }
            }
        }
    }
}
