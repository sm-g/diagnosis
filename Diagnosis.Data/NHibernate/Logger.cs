using Diagnosis.Models;
using log4net;
using System;
using System.Linq;

namespace Diagnosis.Data
{
    [Serializable]
    internal class Logger : IEntityCrudTracker
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Logger));

        public void Insert(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} inserted.", entity.GetType(), entity.Id);
        }

        public void Update(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} updated.", entity.GetType(), entity.Id);
        }

        public void Delete(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} deleted.", entity.GetType(), entity.Id);
        }

        public void Load(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} loaded.", entity.GetType(), entity.Id);
        }
    }
}