using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Domain.Logic.Managers
{
    public abstract class Manager<T> : CleanDisposable where T : class, IModel
    {
        public Manager(IRepository<T> repository)
        {
            Repository = repository;
        }


        protected override void DisposeManagedResources()
        {
            if (Repository != null)
            {
                Repository.Dispose();
                Repository = null;
            }
        }


        protected IRepository<T> Repository { get; set; }
    }
}
