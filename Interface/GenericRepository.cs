using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;

using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using Interface.Models;


namespace Interface
{
    public class GenericRepository<T> where T : class
    {
        protected Entities Db = new Entities();
        private ObjectSet<T> _objectset;
        private ObjectContext context;
        protected ObjectSet<T> ObjectSet
        {
            get
            {
                return ((IObjectContextAdapter)Db).ObjectContext.CreateObjectSet<T>();
            }
        }
        public IQueryable<T> Collection
        {
            get
            {
                return AsQueryable();
            }
        }
        public virtual bool Add(T entity)
        {
            try
            {
                var entry = Db.Entry(entity);
                entry.State = EntityState.Added;
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public virtual void Add(List<T> entities)
        {
            foreach (var entity in entities)
            {
                var entry = Db.Entry(entity);
                entry.State = EntityState.Added;
                Db.SaveChanges();
            }
        }
        public virtual void Save(T entity)
        {
            try
            {
                if (((dynamic)entity).Id == 0)
                {
                    Add(entity);
                }
                else
                {
                    Update(entity);
                }
            }
            catch (Exception)
            {

            }
        }
        public virtual void Save(T entity, bool IDMODE)
        {
            if (((dynamic)entity).ID == 0)
            {
                Add(entity);
            }
            else
            {
                Update(entity, IDMODE);
            }
        }
        public virtual bool Update(T entity)
        {
            Decimal id = ((dynamic)entity).ID;
            var dbObject = ObjectSet.Where("it.Id=" + id).FirstOrDefault();
            var entry = Db.Entry(dbObject);
            entry.CurrentValues.SetValues(entity);
            entry.State = EntityState.Modified;
            Db.SaveChanges();
            return true;
        }
        public virtual void Update(T entity, bool IDMODE)
        {
            Decimal id = ((dynamic)entity).ID;
            var dbObject = ObjectSet.Where("it.Id=" + id).FirstOrDefault();
            var entry = Db.Entry(dbObject);
            entry.CurrentValues.SetValues(entity);
            entry.State = EntityState.Modified;
            Db.SaveChanges();
        }
        public virtual T GetById(Decimal id)
        {
            return ObjectSet.Where("it.Id=" + id).FirstOrDefault();
        }
        public virtual T GetById(Decimal id, bool stat)
        {
            return ObjectSet.Where("it.ID=" + id).FirstOrDefault();
        }
        public virtual T GetById(int id)
        {
            return ObjectSet.Where("it.Id=" + id).FirstOrDefault();
        }
        public virtual void Delete(T entity)
        {
            Decimal id = ((dynamic)entity).ID;
            var dbObject = ObjectSet.Where("it.Id=" + id).FirstOrDefault();
            var entry = Db.Entry(dbObject);
            entry.CurrentValues.SetValues(entity);
            Db.Entry(entity).State = EntityState.Deleted;
            Db.SaveChanges();
        }
        public virtual void Delete(decimal id)
        {
            var entity = ObjectSet.Where("it.Id=" + id).FirstOrDefault();
            if (entity == null) return;

            Db.Entry(entity).State = EntityState.Deleted;
            Db.SaveChanges();
        }

        public virtual bool DeleteRecord(decimal id)
        {
            bool res;
            try
            {
                var entity = ObjectSet.Where("it.Id=" + id).FirstOrDefault();
                if (entity == null)
                    res = false;
                Db.Entry(entity).State = EntityState.Deleted;
                Db.SaveChanges();
                res = true;
            }
            catch (Exception ex)
            {

                res = false;
            }
            return res;
        }
        public virtual T FirstOrDefault()
        {
            return Collection.FirstOrDefault();
        }
        public virtual T LastOrDefaultRecord()
        {
            return ObjectSet.ToList().Skip(ObjectSet.Count() - 1).FirstOrDefault();
        }
        public virtual T LastOrDefaultRecord(Expression<Func<T, bool>> predicate)
        {
            return ObjectSet.Where(predicate).ToList().Skip(ObjectSet.Count() - 1).FirstOrDefault();
        }
        public virtual T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return Collection.FirstOrDefault(predicate);
        }
        public virtual List<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return Collection.Where(predicate).ToList();
        }
        public virtual List<T> GetAll()
        {
            return Collection.ToList();
        }
        public virtual int GetCount(Expression<Func<T, bool>> predicate)
        {
            return Collection.Count(predicate);
        }
        public virtual int GetCount()
        {
            return Collection.Count();
        }
        public virtual IQueryable<T> AsQueryable()
        {
            return ObjectSet.AsQueryable();
        }
        protected virtual void SaveChanges()
        {
            context.SaveChanges();
        }
    }
}
