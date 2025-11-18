using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using ControlDeInventario.Module.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;

namespace ControlDeInventario.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();
        //string name = "MyName";
        //DomainObject1 theObject = ObjectSpace.FirstOrDefault<DomainObject1>(u => u.Name == name);
        //if(theObject == null) {
        //    theObject = ObjectSpace.CreateObject<DomainObject1>();
        //    theObject.Name = name;
        //}



        // The code below creates users and roles for testing purposes only.
        // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
        // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
#if !RELEASE
        // If a role doesn't exist in the database, create this role
        var defaultRole = CreateDefaultRole();
        var adminRole = CreateAdminRole();
        var roleSupplier = CreateRoleSupplier();

        ObjectSpace.CommitChanges(); //This line persists created object(s).

        UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();
        // If a user named 'User' doesn't exist in the database, create this user
        if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null) {
            // Set a password if the standard authentication type is used
            string EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "User", EmptyPassword, (user) => {
                // Add the Users role to the user
                user.Roles.Add(defaultRole);
            });
        }

        // If a user named 'Admin' doesn't exist in the database, create this user
        if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null) {
            // Set a password if the standard authentication type is used
            string EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Admin", EmptyPassword, (user) => {
                // Add the Administrators role to the user
                user.Roles.Add(adminRole);
            });
        }

        // Crear el rol de test si no existe
        if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "RoleSupplier") == null) {
            // Set a password if the standard authentication type is used
            string EmptyPassword = "1234";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "RoleSupplier", EmptyPassword, (user) => {
                // Add the RoleTest role to the user
                user.Roles.Add(roleSupplier);
            });
        }

        ObjectSpace.CommitChanges(); //This line persists created object(s).
#endif
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();

        //Agregar categoria inicial si no existe
        Category defaultCategory = ObjectSpace.FirstOrDefault<Category>(c => c.Name == "General");

        if (defaultCategory == null)
        {
            defaultCategory = ObjectSpace.CreateObject<Category>();
            defaultCategory.Name = "General";
        }

        //Agregar proveedor inicial si no existe
        Supplier defaultSupplier = ObjectSpace.FirstOrDefault<Supplier>(s => s.Name == "Proveedor Inicial");
        if (defaultSupplier == null)
        {
            defaultSupplier = ObjectSpace.CreateObject<Supplier>();
            defaultSupplier.Name = "Proveedor Inicial";
        }

        //Agregar producto inicial si no existe
        Product defaultProduct = ObjectSpace.FirstOrDefault<Product>(p => p.ProductName == "Producto Inicial");
        if (defaultProduct == null)
        {
            defaultProduct = ObjectSpace.CreateObject<Product>();
            defaultProduct.ProductName = "Producto Inicial";
            defaultProduct.Price = 10.0m;
            defaultProduct.Stock = 100;
            defaultProduct.Category = defaultCategory;
            defaultProduct.Supplier = defaultSupplier;
        }


    }
    private PermissionPolicyRole CreateAdminRole() {
        PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
            adminRole.IsAdministrative = true;
        }
        return adminRole;
    }
    private PermissionPolicyRole CreateDefaultRole() {
        PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if(defaultRole == null) {
            defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";

			defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
			defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
			defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
			defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditDataItemPersistent>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermissionFromLambda<AuditDataItemPersistent>(SecurityOperations.Read, a => a.UserId == CurrentUserIdOperator.CurrentUserId().ToString(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditedObjectWeakReference>(SecurityOperations.Read, SecurityPermissionState.Allow);
        }
        return defaultRole;
    }

    private PermissionPolicyRole CreateRoleSupplier()
    {
        PermissionPolicyRole roleTest = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "RoleSupplier");
        if (roleTest == null)
        {
            roleTest = ObjectSpace.CreateObject<PermissionPolicyRole>();
            roleTest.Name = "RoleSupplier";

            //Rol de supplier que solo se le permitira: Hacer cambios en su perfil, ver productos y ver categorias y proveedores, ademas de poder crear proveedores

            // Ver Productos, Categorias y Proveedores (solo lectura)
            roleTest.AddTypePermissionsRecursively<Product>(SecurityOperations.Read, SecurityPermissionState.Allow);
            roleTest.AddTypePermissionsRecursively<Product>(SecurityOperations.Create, SecurityPermissionState.Deny);
            roleTest.AddTypePermissionsRecursively<Product>(SecurityOperations.Write, SecurityPermissionState.Deny);
            roleTest.AddTypePermissionsRecursively<Product>(SecurityOperations.Delete, SecurityPermissionState.Deny);

            roleTest.AddTypePermissionsRecursively<Category>(SecurityOperations.Read, SecurityPermissionState.Allow);
            roleTest.AddTypePermissionsRecursively<Category>(SecurityOperations.Create, SecurityPermissionState.Deny);
            roleTest.AddTypePermissionsRecursively<Category>(SecurityOperations.Write, SecurityPermissionState.Deny);
            roleTest.AddTypePermissionsRecursively<Category>(SecurityOperations.Delete, SecurityPermissionState.Deny);

            roleTest.AddTypePermissionsRecursively<Supplier>(SecurityOperations.Read, SecurityPermissionState.Allow);

            // Permitir crear proveedores pero DENEGAR modificar o borrar
            roleTest.AddTypePermissionsRecursively<Supplier>(SecurityOperations.Create, SecurityPermissionState.Allow);
            roleTest.AddTypePermissionsRecursively<Supplier>(SecurityOperations.Write, SecurityPermissionState.Deny);
            roleTest.AddTypePermissionsRecursively<Supplier>(SecurityOperations.Delete, SecurityPermissionState.Deny);

            //Permisos para gestionar su perfil (solo su propio usuario)
            roleTest.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.ReadWriteAccess, //Da permiso de lectura y escritura
                                                                    u => u.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), //Solo a su propio usuario
                                                                    SecurityPermissionState.Allow); //Permitir

            roleTest.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write,
                                                                    "ChangePasswordOnFirstLogon", //Solo a la propiedad ChangePasswordOnFirstLogon
                                                                    u => u.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(),
                                                                    SecurityPermissionState.Allow);

            roleTest.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write,
                                                                    "StoredPassword", //Solo a la propiedad StoredPassword
                                                                    u => u.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(),
                                                                    SecurityPermissionState.Allow);

            // No permitir ver roles ni datos de auditoría sensibles
            roleTest.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            roleTest.AddTypePermission<AuditDataItemPersistent>(SecurityOperations.Read, SecurityPermissionState.Deny);

            // Permisos de navegación 
            roleTest.AddNavigationPermission(@"Application/NavigationItems/Items/Default", SecurityPermissionState.Allow);
            roleTest.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/Product", SecurityPermissionState.Allow);
            roleTest.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/Product", SecurityPermissionState.Allow);
            roleTest.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/Category", SecurityPermissionState.Allow);
            roleTest.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/Supplier", SecurityPermissionState.Allow);
            roleTest.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);

            // Permitir ModelDifference personales (opcional, para personalización de UI)
            roleTest.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);


        }
        return roleTest;
    }
}
