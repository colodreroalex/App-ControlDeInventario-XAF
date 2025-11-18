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
    [NavigationItem("Product")]
    //[VisibleInDashboards(true)]
    [DefaultProperty(nameof(ProductName))] //Propiedad que se muestra por defecto en las listas
    public class Product : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
      // Use CodeRush to create XPO classes and properties with a few keystrokes.
      // https://docs.devexpress.com/CodeRushForRoslyn/118557

        //Miembros de la clase Product
        private string _productName;
        private decimal _price;
        private int _stock;
        //miebro booleano para la disponibilidad
        private bool _isAvailable;
        private Category _category;
        private Supplier _supplier;

        // Constructor
        public Product(Session session)
            : base(session)
        {
        }

        #region Properties

        [RuleRequiredField(DefaultContexts.Save)] //El campo es obligatorio
        [XafDisplayName("Nombre"), ToolTip("Nombre del Producto")]
        [VisibleInListView(true)] //Esto sirve para mostrar o ocultar campos en la listView
        [Size(200)] //Tamaño maximo del campo
        public string ProductName
        {
            get => _productName;
            set => SetPropertyValue(nameof(ProductName), ref _productName, value);
        }



        [XafDisplayName("Precio"), ToolTip("Precio del Producto")]
        [ModelDefault("DisplayFormat", "{0:C}")] //Formato de moneda con 2 decimales)]
        public decimal Price
        {
            get => _price;
            set => SetPropertyValue(nameof(Price), ref _price, value);
        }



        [XafDisplayName("Inventario"), ToolTip("Cantidad de Unidades Disponibles del Producto")]
        [ImmediatePostData(true)] //Para que el cambio se refleje inmediatamente
        public int Stock
        {
            get => _stock;
            set => SetPropertyValue(nameof(Stock), ref _stock, value);
        }


        
        [XafDisplayName("Disponible"), ToolTip("Indica si el Producto está Disponible para la Venta")]
        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetPropertyValue(nameof(IsAvailable), ref _isAvailable, value);
        }

        [PersistentAlias("Iif(Stock>0, 'Disponible', 'Agotado')")]
        [XafDisplayName("Estado de Disponibilidad"), ToolTip("Texto que indica si el Producto está Disponible o Agotado")]
        public string AvailabilityText => (string)EvaluateAlias(nameof(AvailabilityText));




        [Association("Category-Products")]
        [XafDisplayName("Categoria"), ToolTip("Categoria asociada al Producto")]
        [ReadOnly(true)] //Hacer que el campo sea de solo lectura
        public Category Category
        {
            get => _category;
            set => SetPropertyValue(nameof(Category), ref _category, value);
        }




        [Association("Supplier-Products")]
        [XafDisplayName("Proveedor"), ToolTip("Proveedor del Producto")]
        [ReadOnly(true)]
        public Supplier Supplier
        {
            get => _supplier;
            set => SetPropertyValue(nameof(Supplier), ref _supplier, value);
        }

        

        #endregion




        #region Methods

        // Metodo que se ejecuta despues de la construccion del objeto
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            //UpdateStock(Stock);
        }

        [Action(Caption = "Actualizar Disponibilidad", ConfirmationMessage = "¿Estás seguro de actualizar el inventario?", ImageName = "Refresh", AutoCommit = true)]
        //Metodo para actualizar el stock del producto
        public void UpdateStock()
        {
            // Lógica para actualizar el stock del producto: Verificar si el stock es negativo y cambiar la disponibilidad
            if (Stock < 0)
            {
                Stock = 0;
            }
            IsAvailable = Stock > 0;


        }

        [Action(Caption = "Cambiar Disponibilidad",ConfirmationMessage ="¿Estás seguro?", ImageName = "Attention", AutoCommit = true )]
        
        public void Disponibilidad()
        {   
            this.IsAvailable = !this.IsAvailable;
        }
        #endregion



    }
}
