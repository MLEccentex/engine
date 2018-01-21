﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SolidifyProject.Engine.Infrastructure.Interfaces;
using SolidifyProject.Engine.Infrastructure.Models;
using SolidifyProject.Engine.Infrastructure.Services;
using SolidifyProject.Engine.Test._Fake.Infrastructure.Services;

namespace SolidifyProject.Engine.Test.Infrastructure.Services
{
    [TestFixture]
    public class DataServiceTest
    {
        private List<CustomDataModel> _storage = new List<CustomDataModel>();
        private IContentReaderService<CustomDataModel> _contentReader;
        private IDataService _dataService;

        [OneTimeSetUp]
        public void InitialSetup()
        {
            _storage = new List<CustomDataModel>();
            _contentReader = new ContentReaderServiceFake<CustomDataModel, string>(_storage);
            _dataService= new DataService(_contentReader);
        }

        [TearDown]
        public void Cleanup()
        {
            _storage.Clear();
        }
        
        [Test]
        public async Task RemoveEmptySections()
        {
            _storage.Add(new CustomDataModel{ Id = @".\./.mine.txt", ContentRaw = "mine text" });
            _storage.Add(new CustomDataModel{ Id = "...yours...txt", ContentRaw = "yours text" });
            foreach (var dataModel in _storage)
            {
                dataModel.Parse();
            }
            
            dynamic data = await _dataService.GetDataModelAsync();
            
            Assert.IsNotNull(data);
            Assert.AreEqual("mine text", data.mine);
            Assert.AreEqual("yours text", data.yours);
        }

        [Test]
        public async Task PlainModel()
        {
            _storage.Add(new CustomDataModel{ Id = "mine.txt", ContentRaw = "mine text" });
            _storage.Add(new CustomDataModel{ Id = "yours.txt", ContentRaw = "yours text" });
            foreach (var dataModel in _storage)
            {
                dataModel.Parse();
            }
            
            dynamic data = await _dataService.GetDataModelAsync();
            
            Assert.IsNotNull(data);
            Assert.AreEqual("mine text", data.mine);
            Assert.AreEqual("yours text", data.yours);
        }
        
        [Test]
        public async Task CaseSensitiveModel()
        {
            _storage.Add(new CustomDataModel{ Id = "mine.txt", ContentRaw = "mine text" });
            _storage.Add(new CustomDataModel{ Id = "Mine.txt", ContentRaw = "mine sensitive text" });
            foreach (var dataModel in _storage)
            {
                dataModel.Parse();
            }
            
            dynamic data = await _dataService.GetDataModelAsync();
            
            Assert.IsNotNull(data);
            Assert.AreEqual("mine text", data.mine);
            Assert.AreEqual("mine sensitive text", data.Mine);
        }
        
        [Test]
        public async Task ComplexModel()
        {
            _storage.Add(new CustomDataModel{ Id = @"f1\i1.txt", ContentRaw = "1" });
            _storage.Add(new CustomDataModel{ Id = @"f2\i2.txt", ContentRaw = "2" });
            _storage.Add(new CustomDataModel{ Id = @"f2\i3.txt", ContentRaw = "3" });
            _storage.Add(new CustomDataModel{ Id = @"f3\s3\i4.txt", ContentRaw = "4" });
            _storage.Add(new CustomDataModel{ Id = @"f3\s3\i5.txt", ContentRaw = "5" });
            _storage.Add(new CustomDataModel{ Id = @"f1\i6.txt", ContentRaw = "6" });
            foreach (var dataModel in _storage)
            {
                dataModel.Parse();
            }
            
            dynamic data = await _dataService.GetDataModelAsync();
            
            Assert.IsNotNull(data);
            Assert.AreEqual("1", data.f1.i1);
            Assert.AreEqual("2", data.f2.i2);
            Assert.AreEqual("3", data.f2.i3);
            Assert.AreEqual("4", data.f3.s3.i4);
            Assert.AreEqual("5", data.f3.s3.i5);
            Assert.AreEqual("6", data.f1.i6);
        }
        
        [Test]
        public async Task SpecialPropertyPlainCollection()
        {
            _storage.Add(new CustomDataModel{ Id = "mine.txt", ContentRaw = "mine text" });
            _storage.Add(new CustomDataModel{ Id = "yours.txt", ContentRaw = "yours text" });
            foreach (var dataModel in _storage)
            {
                dataModel.Parse();
            }
            
            dynamic data = await _dataService.GetDataModelAsync();
            
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.__collection);
            Assert.AreEqual("mine text", data.__collection[0]);
            Assert.AreEqual("yours text", data.__collection[1]);
        }
        
        [Test]
        public async Task SpecialProperty2LevelsCollection()
        {
            _storage.Add(new CustomDataModel{ Id = @"f1\i1.txt", ContentRaw = "1" });
            _storage.Add(new CustomDataModel{ Id = @"f1\i2.txt", ContentRaw = "2" });
            _storage.Add(new CustomDataModel{ Id = @"f2\i3.txt", ContentRaw = "3" });
            foreach (var dataModel in _storage)
            {
                dataModel.Parse();
            }
            
            dynamic data = await _dataService.GetDataModelAsync();
            
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.__collection);
            Assert.AreEqual(2, data.__collection.Count);                    // f1, f2
            Assert.AreEqual(2, data.__collection[0].__collection.Count);    // i1, i2
            Assert.AreEqual(1, data.__collection[1].__collection.Count);    // i3
        }
        
        [Test]
        public async Task SpecialProperty3LevelsCollection()
        {
            _storage.Add(new CustomDataModel{ Id = @"f1\i1.txt", ContentRaw = "1" });
            _storage.Add(new CustomDataModel{ Id = @"f2\i2.txt", ContentRaw = "2" });
            _storage.Add(new CustomDataModel{ Id = @"f2\i3.txt", ContentRaw = "3" });
            _storage.Add(new CustomDataModel{ Id = @"f3\s3\i4.txt", ContentRaw = "4" });
            _storage.Add(new CustomDataModel{ Id = @"f3\s3\i5.txt", ContentRaw = "5" });
            _storage.Add(new CustomDataModel{ Id = @"f1\i6.txt", ContentRaw = "6" });
            _storage.Add(new CustomDataModel{ Id = @"f1\i7.txt", ContentRaw = "7" });
            foreach (var dataModel in _storage)
            {
                dataModel.Parse();
            }
            
            dynamic data = await _dataService.GetDataModelAsync();
            
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.__collection);
            Assert.AreEqual(3, data.__collection.Count);
            Assert.AreEqual(3, data.__collection[0].__collection.Count);
            Assert.AreEqual(2, data.__collection[1].__collection.Count);
            Assert.AreEqual(2, data.__collection[2].__collection.Count);
        }
        
        
        protected static object[] _specialPropertyNameIsNotAllowedTestCases =
        {
            new object[] { DataService.COLLECTION_PROPERTY },
            new object[] { $@".\{DataService.COLLECTION_PROPERTY}" },
            new object[] { $@"..\{DataService.COLLECTION_PROPERTY}" },
            new object[] { $@"folder\{DataService.COLLECTION_PROPERTY}" },
            new object[] { $@"folder/{DataService.COLLECTION_PROPERTY}" },
            new object[] { $@"folder\sub\{DataService.COLLECTION_PROPERTY}.txt" },
            new object[] { $@"folder/sub/{DataService.COLLECTION_PROPERTY}.txt" }
        };
        
        [Test]
        [TestCaseSource(nameof(_specialPropertyNameIsNotAllowedTestCases))]
        public void SpecialPropertyNameIsNotAllowed(string id)
        {
            _storage.Add(new CustomDataModel{ Id = id, ContentRaw = "text" });

            Assert.ThrowsAsync<NotSupportedException>(async() => await _dataService.GetDataModelAsync());
        }
    }
}