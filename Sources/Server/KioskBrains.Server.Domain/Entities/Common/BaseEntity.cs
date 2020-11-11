﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KioskBrains.Server.Domain.Entities.Common
{
    public interface IBaseEntity<T> : IBaseEntity
    {
        new T Id { get; set; }
    }

    public interface IBaseEntity
    {
        object Id { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? ModifiedDate { get; set; }
        Guid? CreatedBy { get; set; }
        Guid? ModifiedBy { get; set; }
    }
    public abstract class BaseEntity<T> : IBaseEntity<T>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public virtual T Id { get; set; }

        object IBaseEntity.Id
        {
            get { return Id; }
            set => Id = (T)value;
        }

        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }

        public Guid? CreatedBy { get; set; }

        public Guid? ModifiedBy { get; set; }
    }


    public abstract class BaseEntityManualId<T> : BaseEntity<T>, IBaseEntity<T>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public override T Id { get; set; }
    }
}
