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
      <Database value="i3Forms-Logs" />
      <bufferSize value="50" />
      <evaluator type="log4net.Core.LevelEvaluator">
        <threshold value="ERROR" />
      </evaluator>
    </appender>
```
