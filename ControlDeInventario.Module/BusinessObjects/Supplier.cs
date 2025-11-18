using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ControlDeInventario.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Supplier")]
    public class Supplier : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public Supplier(Session session)
            : base(session)
        {
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        [Association("Supplier-Products")]
        public XPCollection<Product> Products => GetCollection<Product>(nameof(Products));



        public override void AfterConstruction()
        {
            base.AfterConstruction();
            

        }
        
    }
}