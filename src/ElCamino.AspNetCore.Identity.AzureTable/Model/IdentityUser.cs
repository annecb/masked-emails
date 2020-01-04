﻿// MIT License Copyright 2020 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.AzureTable.Helpers;
using Microsoft.Azure.Cosmos.Table;

namespace ElCamino.AspNetCore.Identity.AzureTable.Model
{
    public class IdentityUser : IdentityUser<string>, IGenerateKeys
    {
        public IdentityUser() : base() { }

        public IdentityUser(string userName)
            : this()
        {
            this.UserName = userName;
        }

        /// <summary>
        /// Generates Row, Partition and Id keys.
        /// All are the same in this case
        /// </summary>
        public void GenerateKeys()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                Id = KeyHelper.GenerateUserId();
            }
            RowKey = PeekRowKey();
            PartitionKey = RowKey;
            KeyVersion = KeyHelper.KeyVersion;
        }

        /// <summary>
        /// Generates the RowKey without setting it on the object.
        /// In this case, just returns a key based on username
        /// </summary>
        /// <returns></returns>
        public string PeekRowKey()
        {
            return KeyHelper.GenerateRowKeyUserId(Id);
        }

        public double KeyVersion { get; set; }

        

        public override string UserName
        {
            get => base.UserName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    base.UserName = value.Trim();
                }
            }
        }
    }

    public class IdentityUser<TKey> : Microsoft.AspNetCore.Identity.IdentityUser<TKey>, ITableEntity
        where TKey : IEquatable<TKey>
    {
        public IdentityUser()
        {
        }


        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// LockoutEnd is stored as LockoutEndDateUtc for backwards compat.
        /// </summary>
        [IgnoreProperty]
        public override DateTimeOffset? LockoutEnd
        {
            get
            {
                if(LockoutEndDateUtc.HasValue)
                {
                    return new DateTimeOffset?(new DateTimeOffset(LockoutEndDateUtc.Value));
                }

                return null;
            }
            set
            {
                if(value.HasValue)
                {
                    LockoutEndDateUtc = value.Value.UtcDateTime;
                }
                else
                {
                    LockoutEndDateUtc = null;
                }
            }
        }


        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            TableEntity.ReadUserObject(this, properties, operationContext);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return TableEntity.WriteUserObject(this, operationContext);
        }
    }

    
}
