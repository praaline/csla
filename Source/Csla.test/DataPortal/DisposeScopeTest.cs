﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csla.Configuration;
using Csla.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csla.Test.DataPortal
{
  [TestClass]
  public class DisposeScopeTest
  {

    [TestMethod]
    public void Test_Scope_DoesNotDisposeWithNoLocalScope()
    {
      // CSLA should not dispose of the default service provider.
      IServiceCollection serviceCollection = new ServiceCollection();
      serviceCollection.AddScoped<DisposableClass>();
      serviceCollection.AddCsla(o => o.DataPortal(dp => dp.ClientSideDataPortal((dpo => dpo.UseLocalProxy(lpo => lpo.UseLocalScope = false)))));

      var services = serviceCollection.BuildServiceProvider();
      IDataPortal<ClassA> dataPortal = services.GetRequiredService<IDataPortal<ClassA>>();

      var classA = dataPortal.Fetch();
      var classB = classA.ChildB;

      Assert.AreEqual(classA.DisposableClass.Id, classB.DisposableClass.Id, "Ids must be the same");
      Assert.IsFalse(classA.DisposableClass.IsDisposed, "Object must not be disposed");
    }

    [TestMethod]
    public void Test_Scope_DoesDisposeWithLocalScope()
    {
      // CSLA should dispose of the temporary server-side service provider.
      IServiceCollection serviceCollection = new ServiceCollection();
      serviceCollection.AddScoped<DisposableClass>();
      serviceCollection.AddCsla();

      var services = serviceCollection.BuildServiceProvider();
      IDataPortal<ClassA> dataPortal = services.GetRequiredService<IDataPortal<ClassA>>();

      var classA = dataPortal.Fetch();
      var classB = classA.ChildB;

      Assert.AreEqual(classA.DisposableClass.Id, classB.DisposableClass.Id, "Ids must be the same");
      Assert.IsTrue(classA.DisposableClass.IsDisposed, "Object must not be disposed");
    }
  }

  public class DisposableClass
  : IDisposable
  {
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsDisposed { get; private set; } = false;
    public void Dispose()
    {
      IsDisposed = true;
    }
  }

  public class ClassA : BusinessBase<ClassA>
  {
    public ClassB ChildB { get; set; }
    public DisposableClass DisposableClass { get; set; }

    [Fetch]
    private void Fetch([Inject]DisposableClass disposable, [Inject] IDataPortal<ClassB> classBDataPortal)
    {
      DisposableClass = disposable;

      if (disposable.IsDisposed)
      {
        throw new ObjectDisposedException(nameof(disposable));
      }

      ChildB = classBDataPortal.Fetch();

      if (disposable.IsDisposed)
      {
        throw new ObjectDisposedException(nameof(disposable));
      }
    }
  }

  public class ClassB : BusinessBase<ClassB>
  {
    public DisposableClass DisposableClass { get; set; }
    public Guid Id { get; set; }

    [Fetch]
    private void Fetch([Inject]DisposableClass disposable)
    {
      DisposableClass = disposable;

      if (disposable.IsDisposed)
      {
        throw new ObjectDisposedException(nameof(disposable));
      }
    }
  }
}
