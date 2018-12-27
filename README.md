# log4net.RavenDB
RavenDB (4.0+) Appender for log4net

#Adding the Appender
within the `<log4net>` section

```XML Configuration file
<appender name="RavenDBAppender" type="log4net.RavenDB.RavenDBAppender, log4net.RavenDB">
      <filter type="log4net.Filter.LevelRangeFilter">
        <levelMin value="INFO" />
        <levelMax value="FATAL" />
      </filter>
      <Url value="http://localhost:8080" />
      <Database value="Logs" />
      <bufferSize value="50" />
      <evaluator type="log4net.Core.LevelEvaluator">
        <threshold value="ERROR" />
      </evaluator>
    </appender>
```
The two key properties are the URL and Database.

To Use the appender simply add the appender-ref into the appropriate region.  In the example below we've added it to the `<root>` element:

'''
  <root>
      <level value="INFO" />
      <appender-ref ref="RavenDBAppender" />
      <appender-ref ref="ColoredConsoleAppender" />
    </root>
'''    
