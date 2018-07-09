using System;
using System.Collections.Generic;
using FluentAssertions;
using Toggl.Foundation.MvvmCross.Collections;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.Collections
{
    public sealed class GroupedOrderedListTests
    {
        public sealed class TheConstructor
        {
            DateTimeOffset referenceDate = new DateTimeOffset(2018, 02, 13, 19, 00, 00, TimeSpan.Zero);

            [Fact, LogIfTooSlow]
            public void ListCanBeEmpty()
            {
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length);

                collection.Items.Should().BeEmpty();
            }

            [Fact, LogIfTooSlow]
            public void SetsTheCorrectOrderForInitialItems()
            {
                List<int> list = new List<int> { 4, 7, 8, 3, 1, 2 };

                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 1, 2, 3, 4, 7, 8 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void SetsTheCorrectDescendingOrderForInitialItems()
            {
                List<int> list = new List<int> { 4, 7, 8, 3, 1, 2 };

                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list, descending: true);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 8, 7, 4, 3, 2, 1 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void SetsTheCorrectOrderForInitialItemsWithDates()
            {
                List<DateTimeOffset> list = new List<DateTimeOffset>
                {
                    referenceDate.AddHours(3),
                    referenceDate.AddHours(1),
                    referenceDate.AddHours(-10),
                    referenceDate.AddDays(5)
                };

                var collection = new GroupedOrderedList<DateTimeOffset>(d => d.TimeOfDay, _ => 1, list, descending: true);

                List<List<DateTimeOffset>> expected = new List<List<DateTimeOffset>>
                {
                    new List<DateTimeOffset>
                    {
                        referenceDate.AddDays(5),
                        referenceDate.AddHours(3),
                        referenceDate.AddHours(1),
                        referenceDate.AddHours(-10)
                    }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void GroupsAndOrdersDates()
            {
                List<DateTimeOffset> list = new List<DateTimeOffset>
                {
                    referenceDate.AddHours(3),
                    referenceDate.AddHours(1),
                    referenceDate.AddHours(-10),
                    referenceDate.AddDays(5),
                    referenceDate.AddDays(5).AddHours(2)
                };

                var collection = new GroupedOrderedList<DateTimeOffset>(d => d.TimeOfDay, d => d.Date, list, descending: true);

                List<List<DateTimeOffset>> expected = new List<List<DateTimeOffset>>
                {
                    new List<DateTimeOffset>
                    {
                        referenceDate.AddDays(5).AddHours(2),
                        referenceDate.AddDays(5)
                    },
                    new List<DateTimeOffset>
                    {
                        referenceDate.AddHours(3),
                        referenceDate.AddHours(1),
                        referenceDate.AddHours(-10)
                    }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }
        }

        public sealed class TheItemAtMethod
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfEmpty()
            {
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length);

                Action gettingItem = () => collection.ItemAt(0, 0);
                gettingItem.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Fact, LogIfTooSlow]
            public void ThrowsIfSectionDoesNotExist()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                Action gettingItem = () => collection.ItemAt(2, 0);

                gettingItem.Should().Throw<ArgumentOutOfRangeException>();
            }


            [Fact, LogIfTooSlow]
            public void ThrowsIfIndexOutOfRange()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };

                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                Action gettingItem = () => collection.ItemAt(0, 6);

                gettingItem.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Fact, LogIfTooSlow]
            public void ReturnsTheCorrectItem()
            {
                DateTimeOffset referenceDate = new DateTimeOffset(2018, 02, 13, 19, 00, 00, TimeSpan.Zero);
                List<DateTimeOffset> list = new List<DateTimeOffset>
                {
                    referenceDate.AddHours(3),
                    referenceDate.AddHours(1),
                    referenceDate.AddHours(-10),
                    referenceDate.AddDays(5),
                    referenceDate.AddDays(5).AddHours(2)
                };
                var collection = new GroupedOrderedList<DateTimeOffset>(d => d.TimeOfDay, d => d.Date, list, descending: true);

                collection.ItemAt(1, 0).Should().Be(referenceDate.AddDays(5));
            }
        }

        public sealed class TheRemoveItemMethod
        {
            [Fact, LogIfTooSlow]
            public void ReturnsNullIfEmpty()
            {
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length);

                collection.RemoveItem(5).Should().Be(null);
            }

            [Fact, LogIfTooSlow]
            public void ReturnsNullIfItemCantBeFound()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItem(4).Should().Be(null);
            }

            [Fact, LogIfTooSlow]
            public void RemovesCorrectItem()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItem(3);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40, 70 },
                    new List<int> { 1, 2, 8 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void ReturnsCorrectSectionedIndex()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                var expected = new SectionedIndex { Section = 1, Row = 0 };
                collection.RemoveItem(40).Should().Be(expected);
            }

            [Fact, LogIfTooSlow]
            public void RemovesSectionIfEmpty()
            {
                List<int> list = new List<int> { 40, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItem(40);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 1, 2, 3 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void CanRemoveLastItem()
            {
                List<int> list = new List<int> { 40, 70, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItem(70);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40 },
                    new List<int> { 1, 2, 3 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void CanRemoveFirstItem()
            {
                List<int> list = new List<int> { 40, 70, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItem(1);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40, 70 },
                    new List<int> { 2, 3 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }
        }

        public sealed class TheRemoveItemAtMethod
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfEmpty()
            {
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length);

                Action remoteItemAt = () => collection.RemoveItemAt(0, 0);

                remoteItemAt.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Fact, LogIfTooSlow]
            public void ThrowsIfIndexOutOfRange()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                Action remoteItemAt = () => collection.RemoveItemAt(1, 4);

                remoteItemAt.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Fact, LogIfTooSlow]
            public void RemovesCorrectItem()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItemAt(0, 3);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40, 70 },
                    new List<int> { 1, 2, 3 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void ReturnsCorrectItem()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                var expected = new SectionedIndex { Section = 1, Row = 0 };
                collection.RemoveItemAt(1, 0).Should().Be(40);
            }

            [Fact, LogIfTooSlow]
            public void RemovesSectionIfEmpty()
            {
                List<int> list = new List<int> { 40, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItemAt(1, 0);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 1, 2, 3 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void CanRemoteLastItem()
            {
                List<int> list = new List<int> { 40, 70, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItemAt(1, 1);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40 },
                    new List<int> { 1, 2, 3 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void CanRemoteFirstItem()
            {
                List<int> list = new List<int> { 40, 70, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.RemoveItemAt(0, 0);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40, 70 },
                    new List<int> { 2, 3 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }
        }

        public sealed class TheInsertItemMethod
        {
            [Fact, LogIfTooSlow]
            public void InsertsElementInCorrectOrder()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.InsertItem(4);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40, 70 },
                    new List<int> { 1, 2, 3, 4, 8 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void CreatesNewSectionIfNeeded()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.InsertItem(200);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 200 },
                    new List<int> { 40, 70 },
                    new List<int> { 1, 2, 3, 8 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void ReturnsCorrectSectionIndex()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                var expected = new SectionedIndex { Section = 0, Row = 3 };

                collection.InsertItem(4).Should().Be(expected);
            }

            [Fact, LogIfTooSlow]
            public void CreatesANewSectionIfNeeded()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.InsertItem(400);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 400 },
                    new List<int> { 40, 70 },
                    new List<int> { 1, 2, 3, 8 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void CanInsertLastItem()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.InsertItem(9);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 40, 70 },
                    new List<int> { 1, 2, 3, 8, 9 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void CanInsertFirstItem()
            {
                List<int> list = new List<int> { 40, 70, 8, 3, 1, 2 };
                var collection = new GroupedOrderedList<int>(i => i, i => i.ToString().Length, list);

                collection.InsertItem(10);

                List<List<int>> expected = new List<List<int>>
                {
                    new List<int> { 10, 40, 70 },
                    new List<int> { 1, 2, 3, 8 }
                };
                collection.Items.Should().BeEquivalentTo(expected);
            }
        }
    }
}
