using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SV.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.SqlServer.Server;
using static Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SVTests
{
    [TestClass]
    public class RealStateControllerTests
    {
        private RealStateFormsController _controller;
        private Mock<InscripcionesBrDbContext> _mockDbContext;

        [TestInitialize]
        public void Setup()
        {
            _mockDbContext = new Mock<InscripcionesBrDbContext>();
            _controller = new RealStateFormsController(_mockDbContext.Object);
        }


        [TestMethod]
        public async Task Create_ValidForm_RedirectsToIndex()
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "AttentionNumber", new StringValues("12") },
                { "NatureOfTheDeed", new  StringValues("Compraventa") },
                { "Commune", new StringValues("Santiago") },
                { "Block", new StringValues("32") },
                { "Property", new StringValues("123") },
                { "Sheets", new StringValues("2") },
                { "InscriptionDate", new StringValues("2022-06-22T19:15") },
                { "InscriptionNumber", new StringValues("2") },
                { "rutSeller", new StringValues(new string[] { "10.534.906-8" }) },
                { "ownershipPercentageSeller", new StringValues(new string[] {"30"}) },
                { "uncreditedClickedSeller", new StringValues(new string[] {"false"}) },
                { "rutBuyer", new StringValues(new string[] { "20.427.455-K", "20.428.706-6" }) },
                { "ownershipPercentageBuyer", new StringValues(new string[] {"10", "20"}) },
                { "uncreditedClickedBuyer", new StringValues(new string[] {"false", "false"}) }
            });

            RealStateForm rsform = new()
            {
                AttentionNumber = 12,
                NatureOfTheDeed = "Compraventa",
                Commune = "Santiago",
                Block = "32",
                Property = "123",
                InscriptionDate = DateTime.Parse("2022-06-22T19:15"),
                InscriptionNumber = 2
            };

            var rsforms = new List<RealStateForm>
            {
                new RealStateForm
                {
                    AttentionNumber = 11,
                    NatureOfTheDeed = "Compraventa",
                    Commune = "Las Condes",
                    Block = "32",
                    Property = "123",
                    InscriptionDate = DateTime.Parse("2022-06-22T19:15"),
                    InscriptionNumber = 3
                },
                rsform
            };

            List<Person> people = new()
            {
                new Person { Forms = rsform, Rut = "10.534.906-8", OwnershipPercentage = 30, UncreditedOwnership = false, Seller = true, Heir = false  },
                new Person { Forms = rsform, Rut = "20.427.455-K", OwnershipPercentage = 10, UncreditedOwnership = false, Seller = false, Heir = true  },
                new Person { Forms = rsform, Rut = "20.428.706-6", OwnershipPercentage = 20, UncreditedOwnership = false, Seller = false, Heir = true  }
            };

            List<MultiOwner> multiOwner = new()
            {
                new MultiOwner { Rut = "10.534.906-8", OwnershipPercentage = 30, Commune = "Las Condes", Block = "32", Property = "123", Sheets = 1, 
                    InscriptionDate = DateTime.Parse("2021-06-22T19:15"), InscriptionNumber = 1, ValidityYearBegin = 2021, ValidityYearFinish = null }
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = form } }
            };

            var _mockDbSetRSF = new Mock<DbSet<RealStateForm>>();
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.Provider).Returns(rsforms.AsQueryable().Provider);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.Expression).Returns(rsforms.AsQueryable().Expression);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.ElementType).Returns(rsforms.AsQueryable().ElementType);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.GetEnumerator()).Returns(() => rsforms.AsQueryable().GetEnumerator());

            var _mockDbSetP = new Mock<DbSet<Person>>();
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.AsQueryable().Provider);
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.AsQueryable().Expression);
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.AsQueryable().ElementType);
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(() => people.AsQueryable().GetEnumerator());

            var _mockDbSetMO = new Mock<DbSet<MultiOwner>>();
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.Provider).Returns(multiOwner.AsQueryable().Provider);
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.Expression).Returns(multiOwner.AsQueryable().Expression);
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.ElementType).Returns(multiOwner.AsQueryable().ElementType);
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.GetEnumerator()).Returns(() => multiOwner.AsQueryable().GetEnumerator());

            _mockDbContext.Setup(c => c.RealStateForms).Returns(_mockDbSetRSF.Object);
            _mockDbContext.Setup(c => c.People).Returns(_mockDbSetP.Object);
            _mockDbContext.Setup(c => c.MultiOwners).Returns(_mockDbSetMO.Object);

            var result = await _controller.Create(rsform);

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            _mockDbContext.Verify(c => c.Add(It.IsAny<RealStateForm>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_InvalidSellerForm()
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "AttentionNumber", new StringValues("12") },
                { "NatureOfTheDeed", new  StringValues("Compraventa") },
                { "Commune", new StringValues("Santiago") },
                { "Block", new StringValues("32") },
                { "Property", new StringValues("123") },
                { "Sheets", new StringValues("2") },
                { "InscriptionDate", new StringValues("2022-06-22T19:15") },
                { "InscriptionNumber", new StringValues("2") },
                { "rutSeller", new StringValues(Array.Empty<string>()) },
                { "ownershipPercentageSeller", new StringValues(new string[] {"30"}) },
                { "uncreditedClickedSeller", new StringValues(new string[] {"false"}) },
                { "rutBuyer", new StringValues(new string[] { "20.427.455-K", "20.428.706-6" }) },
                { "ownershipPercentageBuyer", new StringValues(new string[] {"10", "20"}) },
                { "uncreditedClickedBuyer", new StringValues(new string[] {"false", "false"}) }
            });

            RealStateForm rsform = new()
            {
                AttentionNumber = 12,
                NatureOfTheDeed = "Compraventa",
                Commune = "Santiago",
                Block = "32",
                Property = "123",
                InscriptionDate = DateTime.Parse("2022-06-22T19:15"),
                InscriptionNumber = 2
            };

            var rsforms = new List<RealStateForm>
            {
                new RealStateForm
                {
                    AttentionNumber = 11,
                    NatureOfTheDeed = "Compraventa",
                    Commune = "Las Condes",
                    Block = "32",
                    Property = "123",
                    InscriptionDate = DateTime.Parse("2022-06-22T19:15"),
                    InscriptionNumber = 3
                },
                rsform
            };

            List<Person> people = new()
            {
                new Person { Forms = rsform, Rut = "20.427.455-K", OwnershipPercentage = 10, UncreditedOwnership = false, Seller = false, Heir = true  },
                new Person { Forms = rsform, Rut = "20.428.706-6", OwnershipPercentage = 20, UncreditedOwnership = false, Seller = false, Heir = true  }
            };

            List<MultiOwner> multiOwner = new()
            {
                new MultiOwner { Rut = "10.534.906-8", OwnershipPercentage = 30, Commune = "Las Condes", Block = "32", Property = "123", Sheets = 1,
                    InscriptionDate = DateTime.Parse("2021-06-22T19:15"), InscriptionNumber = 1, ValidityYearBegin = 2021, ValidityYearFinish = null }
            };

            List<Commune> communes = new()
            {
                new Commune { Name = "Algarrobo" }
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = form } }
            };

            var _mockDbSetRSF = new Mock<DbSet<RealStateForm>>();
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.Provider).Returns(rsforms.AsQueryable().Provider);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.Expression).Returns(rsforms.AsQueryable().Expression);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.ElementType).Returns(rsforms.AsQueryable().ElementType);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.GetEnumerator()).Returns(() => rsforms.AsQueryable().GetEnumerator());

            var _mockDbSetP = new Mock<DbSet<Person>>();
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.AsQueryable().Provider);
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.AsQueryable().Expression);
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.AsQueryable().ElementType);
            _mockDbSetP.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(() => people.AsQueryable().GetEnumerator());

            var _mockDbSetMO = new Mock<DbSet<MultiOwner>>();
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.Provider).Returns(multiOwner.AsQueryable().Provider);
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.Expression).Returns(multiOwner.AsQueryable().Expression);
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.ElementType).Returns(multiOwner.AsQueryable().ElementType);
            _mockDbSetMO.As<IQueryable<MultiOwner>>().Setup(m => m.GetEnumerator()).Returns(() => multiOwner.AsQueryable().GetEnumerator());

            var _mockDbSetC = new Mock<DbSet<Commune>>();
            _mockDbSetC.As<IQueryable<Commune>>().Setup(m => m.Provider).Returns(communes.AsQueryable().Provider);
            _mockDbSetC.As<IQueryable<Commune>>().Setup(m => m.Expression).Returns(communes.AsQueryable().Expression);
            _mockDbSetC.As<IQueryable<Commune>>().Setup(m => m.ElementType).Returns(communes.AsQueryable().ElementType);
            _mockDbSetC.As<IQueryable<Commune>>().Setup(m => m.GetEnumerator()).Returns(() => communes.AsQueryable().GetEnumerator());

            _mockDbContext.Setup(c => c.RealStateForms).Returns(_mockDbSetRSF.Object);
            _mockDbContext.Setup(c => c.People).Returns(_mockDbSetP.Object);
            _mockDbContext.Setup(c => c.MultiOwners).Returns(_mockDbSetMO.Object);
            _mockDbContext.Setup(c => c.Commune).Returns(_mockDbSetC.Object);

            var result = await _controller.Create(rsform);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(RealStateForm));

        }

        [TestMethod]
        public async Task Index_ReturnsViewWithRealStateForms()
        {
            var rsforms = new List<RealStateForm>
            {
                new RealStateForm { AttentionNumber = 1 },
                new RealStateForm { AttentionNumber = 2 }
            };

            var _mockDbSetRSF = new Mock<DbSet<RealStateForm>>();
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.Provider).Returns(rsforms.AsQueryable().Provider);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.Expression).Returns(rsforms.AsQueryable().Expression);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.ElementType).Returns(rsforms.AsQueryable().ElementType);
            _mockDbSetRSF.As<IQueryable<RealStateForm>>().Setup(m => m.GetEnumerator()).Returns(() => rsforms.AsQueryable().GetEnumerator());
            _mockDbSetRSF.As<IAsyncEnumerable<RealStateForm>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<RealStateForm>(rsforms.GetEnumerator()));

            _mockDbContext.Setup(c => c.RealStateForms).Returns(_mockDbSetRSF.Object);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsNotNull(viewResult.Model);
            var model = viewResult.Model as List<RealStateForm>;
            CollectionAssert.AreEqual(rsforms, model);
        }

        public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public TestAsyncEnumerator(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            public T Current => _enumerator.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_enumerator.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _enumerator.Dispose();
                return new ValueTask();
            }
        }

    }
}