# log4net.RavenDB
RavenDB (4.0+) Appender for log4net

#Adding the Appender
within the `<log4net>` section

![Build Status](https://ci.appveyor.com/api/projects/status/n5e60c96ek11p14j?svg=true)

```XML Configuration file
<appender name="RavenDBAppender" type="log4net.RavenDB.RavenDBAppender, log4net.RavenDB">
      <Url value="http://localhost:8080" />
      <Database value="Logs" />
</appender>
```
The two key properties are the URL and Database.

To Use the appender simply add the appender-ref into the appropriate region.  In the example below we've added it to the `<root>` element:

```XML
<root>
      <level value="INFO" />
      <appender-ref ref="RavenDBAppender" />
</root>
```    
