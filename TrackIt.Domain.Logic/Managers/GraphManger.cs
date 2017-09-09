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
    public abstract class GraphManager<TParent, TChild> : Manager<TParent>
        where TParent : class, IModel 
        where TChild : class, IModel
    {
        public GraphManager(IRepository<TParent> repository, IRepository<TChild> childRepository)
            : base(repository)
        {
            ChildRepository = childRepository;
        }


        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            if (ChildRepository != null)
            {
                ChildRepository.Dispose();
                ChildRepository = null;
            }
        }


        protected IRepository<TChild> ChildRepository { get; set; }
    }
}
