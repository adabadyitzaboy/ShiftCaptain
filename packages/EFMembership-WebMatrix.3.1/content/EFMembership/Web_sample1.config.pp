<?xml version="1.0"?>
<configuration>
  <connectionStrings>
    <add name="DefaultConnection" connectionString="Data Source=(LocalDb)\v11.0;Initial Catalog=aspnet-WebMatrix_Sample1-20140412113758;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\aspnet-WebMatrix_Sample1-20140412113758.mdf" providerName="System.Data.SqlClient" />
  </connectionStrings>

  <system.web>
    <membership defaultProvider="MyCustomMembershipProvider">
      <providers>
        <clear />
        <add name="MyCustomMembershipProvider"
             type="$rootnamespace$.MyMembership, $assemblyname$"
             connectionStringName="DefaultConnection"
             tablePrefix="tbl_webmatrix_"
             enablePasswordRetrieval="false"
             enablePasswordReset="true"
             requiresQuestionAndAnswer="false"
             requiresUniqueEmail="false"
             maxInvalidPasswordAttempts="5"
             minRequiredPasswordLength="6"
             minRequiredNonalphanumericCharacters="0"
             requiresEmail="false"
             passwordAttemptWindow="10"
             applicationName="/"></add>
      </providers>
    </membership>
    <roleManager enabled="true" defaultProvider="MyCustomRoleProvider">
      <providers>
        <clear />
        <add name="MyCustomRoleProvider"
             membershipProvider="MyCustomMembershipProvider"
             type="$rootnamespace$.MyRole, $assemblyname$"
             connectionStringName="DefaultConnection"
             tablePrefix="tbl_webmatrix_"
             applicationName="/" />
      </providers>
    </roleManager>

  </system.web>
  <entityFramework>
    <defaultConnectionFactory type="System.Data.Entity.Infrastructure.LocalDbConnectionFactory, EntityFramework">
      <parameters>
        <parameter value="v11.0" />
      </parameters>
    </defaultConnectionFactory>
    <providers>
      <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
    </providers>
  </entityFramework>
</configuration>
