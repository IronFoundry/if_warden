﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IronFoundry.Warden.Containers;
using IronFoundry.Container.Utilities;
using NSubstitute;
using Xunit;
using Newtonsoft.Json;
using IronFoundry.Container.Internal;

namespace IronFoundry.Container
{
    public class ContainerServiceTests
    {
        string ContainerBasePath { get; set; }
        string ContainerUserGroup { get; set; }
        FileSystemManager FileSystem { get; set; }
        ContainerHandleHelper HandleHelper { get; set; }
        IProcessRunner ProcessRunner { get; set; }
        IContainerHostService ContainerHostService { get; set; }
        IContainerHostClient ContainerHostClient { get; set; }
        IContainerPropertyService ContainerPropertiesService { get; set; }
        IUserManager UserManager { get; set; }
        ILocalTcpPortManager TcpPortManager { get; set; }
        ContainerService Service { get; set; }

        public ContainerServiceTests()
        {
            ContainerBasePath = @"C:\Containers";
            ContainerUserGroup = "ContainerUsers";

            ContainerPropertiesService = Substitute.For<IContainerPropertyService>();

            FileSystem = Substitute.For<FileSystemManager>();

            HandleHelper = Substitute.For<ContainerHandleHelper>();
            HandleHelper.GenerateId(null).ReturnsForAnyArgs("DEADBEEF");

            ProcessRunner = Substitute.For<IProcessRunner>();
            TcpPortManager = Substitute.For<ILocalTcpPortManager>();
            UserManager = Substitute.For<IUserManager>();

            ContainerHostClient = Substitute.For<IContainerHostClient>();

            ContainerHostService = Substitute.For<IContainerHostService>();
            ContainerHostService.StartContainerHost(null, null, null, null)
                .ReturnsForAnyArgs(ContainerHostClient);

            UserManager.CreateUser(null).ReturnsForAnyArgs(new NetworkCredential("username", "password"));

            Service = new ContainerService(HandleHelper, UserManager, FileSystem, ContainerPropertiesService, TcpPortManager, ProcessRunner, ContainerHostService, ContainerBasePath);
        }

        public class CreateContainer : ContainerServiceTests
        {
            [Fact]
            public void WhenSpecIsNull_Throws()
            {
                var ex = Record.Exception(() => Service.CreateContainer(null));

                Assert.IsAssignableFrom<ArgumentException>(ex);
            }

            [Fact]
            public void UsesProvidedHandle()
            {
                var spec = new ContainerSpec
                {
                    Handle = "container-handle",
                };

                var container = Service.CreateContainer(spec);

                Assert.Equal("container-handle", container.Handle);
            }

            [InlineData(null)]
            [InlineData("")]
            [Theory]
            public void WhenHandleIsNotProvided_GeneratesHandle(string handle)
            {
                var expectedHandle = Guid.NewGuid().ToString("N");
                HandleHelper.GenerateHandle().Returns(expectedHandle);
                var spec = new ContainerSpec
                {
                    Handle = handle,
                };

                var container = Service.CreateContainer(spec);

                Assert.NotEqual(handle, container.Handle);
                Assert.Equal(expectedHandle, container.Handle);
            }

            [Fact]
            public void GeneratesIdFromHandle()
            {
                HandleHelper.GenerateId("handle").Returns("derived-id");
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                };

                var container = Service.CreateContainer(spec);

                Assert.Equal("derived-id", container.Id);
            }

            [Fact]
            public void CreatesContainerSpecificUser()
            {
                UserManager.CreateUser("").ReturnsForAnyArgs(new NetworkCredential());
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                };

                Service.CreateContainer(spec);

                UserManager.Received(1).CreateUser("c_DEADBEEF");
            }

            [Fact]
            public void CreatesContainerSpecificDirectory()
            {
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                };

                Service.CreateContainer(spec);

                var expectedPath = Path.Combine(ContainerBasePath, "DEADBEEF");
                FileSystem.Received(1).CreateDirectory(expectedPath, Arg.Any<IEnumerable<UserAccess>>());
            }

            [Fact]
            public void CreatesContainerSpecificHost()
            {
                var expectedCredentials = new NetworkCredential();
                UserManager.CreateUser("").ReturnsForAnyArgs(expectedCredentials);
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                };

                Service.CreateContainer(spec);

                ContainerHostService.Received(1).StartContainerHost(Arg.Any<string>(), Arg.Any<IContainerDirectory>(), Arg.Any<JobObject>(), expectedCredentials);
            }

            [Fact]
            public void SetsProperties()
            {
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                    Properties = new Dictionary<string,string>
                    {
                        { "name1", "value1" },
                        { "name2", "value2" },
                    },
                };

                var container = Service.CreateContainer(spec);

                ContainerPropertiesService.Received(1).SetProperties(container, spec.Properties);
            }

            [Fact]
            public void CleansUpWhenItFails()
            {
                var spec = new ContainerSpec
                {
                    Handle = "handle",
                };

                ContainerHostService.StartContainerHost(null, null, null, null)
                    .ThrowsForAnyArgs(new Exception());

                try
                {
                    Service.CreateContainer(spec);
                }
                catch (Exception)
                {
                    // Expect this exception.
                }

                // Created and deleted the user
                UserManager.Received(1).CreateUser(Arg.Any<string>());
                UserManager.Received(1).DeleteUser(Arg.Any<string>());
            }
        }

        public class WithContainer : ContainerServiceTests
        {
            string Handle { get; set; }
            IContainer Container { get; set; }
            
            public WithContainer()
            {
                Handle = "KnownHandle";
                var spec = new ContainerSpec
                {
                    Handle = Handle,
                };

                Container = Service.CreateContainer(spec);
            }

            public class GetContainers : WithContainer
            {
                [Fact]
                public void CreateShouldAddToTheList()
                {
                    var containers = Service.GetContainers();
                    Assert.Collection(containers,
                        x => Assert.Same(Container, x)
                    );
                }
            }

            public class GetContainerByHandle : WithContainer
            {
                [Fact]
                public void CanGetContainerByHandle()
                {
                    var container = Service.GetContainerByHandle(Handle);

                    Assert.Same(Container, container);
                }

                [Fact]
                public void IsNotCaseSensitive()
                {
                    var container = Service.GetContainerByHandle("knOwnhAndlE");

                    Assert.Same(Container, container);
                }

                [Fact]
                public void WhenHandleDoesNotExist_ReturnsNull()
                {
                    var container = Service.GetContainerByHandle("UnknownHandle");

                    Assert.Null(container);
                }
            }
        }
    }
}
