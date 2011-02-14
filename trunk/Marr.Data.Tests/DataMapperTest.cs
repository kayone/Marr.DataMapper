﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using Rhino.Mocks;
using Marr.Data.Tests.Entities;

namespace Marr.Data.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class DataMapperTest : TestBase
    {
        [TestMethod]
        public void Find_ShouldMapToEntity()
        {
            // Arrange
            StubResultSet rs = new StubResultSet("ID", "Name", "Age", "IsHappy", "BirthDate");
            rs.AddRow(1, "Jordan", 33, true, new DateTime(1977, 1, 22));

            // Act
            var db = CreateDB_ForQuery(rs);
            Person person = db.Find<Person>("sql...");

            // Assert
            Assert.IsNotNull(person);

            Assert.AreEqual(1, person.ID);
            Assert.AreEqual("Jordan", person.Name);
            Assert.AreEqual(33, person.Age);
            Assert.AreEqual(true, person.IsHappy);
            Assert.AreEqual(new DateTime(1977, 1, 22), person.BirthDate);
        }

        [TestMethod]
        public void Find_WithNoRows_ShouldReturnNull()
        {
            // Arrange
            StubResultSet rs = new StubResultSet("ID", "Name", "Age", "IsHappy", "BirthDate");

            // Act
            var db = CreateDB_ForQuery(rs);
            Person person = db.Find<Person>("sql...");

            // Assert
            Assert.IsNull(person);
        }

        [TestMethod]
        public void Find_WithNoRows_PassingInObject_ShouldReturnObject()
        {
            // Arrange
            StubResultSet rs = new StubResultSet("ID", "Name", "Age", "IsHappy", "BirthDate");

            // Act
            var db = CreateDB_ForQuery(rs);
            Person person = new Person { ID = 5 };
            db.Find<Person>("sql...", person);

            // Assert
            Assert.IsNotNull(person);
            Assert.AreEqual(5, person.ID);
        }

        [TestMethod]
        public void Query_ShouldMapToList()
        {
            // Arrange
            StubResultSet rs = new StubResultSet("ID", "Name", "Age", "IsHappy", "BirthDate");
            rs.AddRow(1, "Jordan", 33, true, new DateTime(1977, 1, 22));
            rs.AddRow(2, "Amyme", 31, false, new DateTime(1979, 10, 19));

            // Act
            var db = CreateDB_ForQuery(rs);
            var people = db.Query<Person>("sql...");

            // Assert
            Assert.IsTrue(people.Count == 2);

            Person jordan = people[0];
            Assert.AreEqual(1, jordan.ID);
            Assert.AreEqual("Jordan", jordan.Name);
            Assert.AreEqual(33, jordan.Age);
            Assert.AreEqual(true, jordan.IsHappy);
            Assert.AreEqual(new DateTime(1977, 1, 22), jordan.BirthDate);

            Person amyme = people[1];
            Assert.AreEqual(2, amyme.ID);
            Assert.AreEqual("Amyme", amyme.Name);
            Assert.AreEqual(31, amyme.Age);
            Assert.AreEqual(false, amyme.IsHappy);
            Assert.AreEqual(new DateTime(1979, 10, 19), amyme.BirthDate);
        }

        [TestMethod]
        public void QueryToGraph_WithNestedRelationships_ShouldMapToGraph()
        {
            // Arrange
            StubResultSet rs = new StubResultSet("ID", "OrderName", "OrderItemID", "ItemDescription", "Price", "AmountPaid");
            rs.AddRow(1, "Order1", 50, "Red car", 100.35m, DBNull.Value);
            rs.AddRow(1, "Order1", 51, "Blue wagon", 44.87m, DBNull.Value);
            rs.AddRow(2, "Order2", 60, "Guitar", 1500.50m, 1500.50m);
            rs.AddRow(2, "Order2", 61, "Bass", 2380.00m, 50.00m);

            // Act
            var db = CreateDB_ForQuery(rs);
            List<Order> orders = db.QueryToGraph<Order>("sql...");

            // Assert
            Assert.IsTrue(orders.Count == 2);
            Order order1 = orders[0];
            Order order2 = orders[1];
            Assert.IsTrue(order1.OrderItems.Count == 2);
            Assert.IsTrue(order2.OrderItems.Count == 2);

            // Order 1
            Assert.AreEqual(1, order1.ID);
            Assert.AreEqual("Order1", order1.OrderName);

            // Order 1 -> Item 1
            Assert.AreEqual(50, order1.OrderItems[0].ID);
            Assert.AreEqual("Red car", order1.OrderItems[0].ItemDescription);
            Assert.AreEqual(100.35m, order1.OrderItems[0].Price);
            Assert.IsNull(order1.OrderItems[0].ItemReceipt.AmountPaid);

            // Order 1 -> Item 2
            Assert.AreEqual(51, order1.OrderItems[1].ID);
            Assert.AreEqual("Blue wagon", order1.OrderItems[1].ItemDescription);
            Assert.AreEqual(44.87m, order1.OrderItems[1].Price);
            Assert.IsNull(order1.OrderItems[1].ItemReceipt.AmountPaid);

            // Order 2 -> Item 1
            Assert.AreEqual(60, order2.OrderItems[0].ID);
            Assert.AreEqual("Guitar", order2.OrderItems[0].ItemDescription);
            Assert.AreEqual(1500.50m, order2.OrderItems[0].Price);
            Assert.AreEqual(1500.50m, order2.OrderItems[0].ItemReceipt.AmountPaid);

            // Order 2 -> Item 2
            Assert.AreEqual(61, order2.OrderItems[1].ID);
            Assert.AreEqual("Bass", order2.OrderItems[1].ItemDescription);
            Assert.AreEqual(2380.00m, order2.OrderItems[1].Price);
            Assert.AreEqual(50.00m, order2.OrderItems[1].ItemReceipt.AmountPaid);
        }

        [TestMethod]
        public void QueryToGraph_WhenNotSortedByParent_ShouldThrowException()
        {
            // Arrange
            StubResultSet rs = new StubResultSet("ID", "OrderName", "OrderItemID", "ItemDescription", "Price", "AmountPaid");
            rs.AddRow(1, "Order1", 50, "Red car", 100.35m, DBNull.Value);
            rs.AddRow(2, "Order2", 60, "Guitar", 1500.50m, 1500.50m);       // Reversed order
            rs.AddRow(1, "Order1", 51, "Blue wagon", 44.87m, DBNull.Value); // Reversed order
            rs.AddRow(2, "Order2", 61, "Bass", 2380.00m, 50.00m);

            var db = CreateDB_ForQuery(rs);

            Exception expectedException = null;

            // Act
            try
            {
                List<Order> orders = db.QueryToGraph<Order>("sql...");
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }            

            // Assert
            Assert.IsNotNull(expectedException, "The DataMapper EntityGraph should have thrown exception because the QueryToGraph results were not properly sorted.");
            Assert.IsInstanceOfType(expectedException, typeof(DataMappingException));
        }

        [TestMethod]
        public void Update_ShouldAddFiveParameters_And_ExecuteNonQuery()
        {
            // Arrange
            Person person = new Person();
            person.ID = 1;
            person.Name = "Jordan";
            person.Age = 33;
            person.IsHappy = true;
            person.BirthDate = new DateTime(1977, 1, 22);

            var db = CreateDB_ForUpdate();
            db.Command.Parameters
                .Expect(p => p.Add(null))
                .IgnoreArguments()
                .Repeat.Times(5)
                .Return(0);

            // Act
            db.Update<Person>(person, "sql...");

            // Assert
            db.Command.Parameters.VerifyAllExpectations();
            db.Command.VerifyAllExpectations();
        }

        [TestMethod]
        public void Insert_ShouldAddFiveParameters_And_ExecuteScalar_AndSetReturnValue()
        {
            // Arrange
            Person person = new Person();
            person.Name = "Jordan";
            person.Age = 33;
            person.IsHappy = true;
            person.BirthDate = new DateTime(1977, 1, 22);

            var db = CreateDB_ForInsert();
            db.Command
                .Expect(c => c.ExecuteScalar())
                .Return(55);

            db.Command.Parameters
                .Expect(p => p.Add(null))
                .IgnoreArguments()
                .Repeat.Times(4)
                .Return(0);

            // Act
            db.Insert<Person>(person, "sql...");

            // Assert
            db.Command.Parameters.VerifyAllExpectations();
            db.Command.VerifyAllExpectations();
            Assert.AreEqual(55, person.ID);
        }
    }
}
