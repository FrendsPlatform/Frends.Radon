- [Frends.Radon](#frends-radon)
    - [Installing](#installing)
    - [Building](#building)
    - [Contributing](#contributing)
    - [Technical overview](#techinal-overview)
        - [Filtering](#filtering)
        - [Email templates](#email-templates)
        - [External events](#external-events)
    - [Parameter reference](#parameter-reference)
        - [Frends.Radon.Execute](#frendsradonexecute)
        - [Frends.Radon.SendMail](#frendsradonsendmail)        

# Frends Radon

FRENDS Radon is a FRENDS task which scans the Windows eventlog and send email alerts based on the found events.

FRENDS Radon tries to keep track of what events it has reported earlier. This is done by storing the timestamp and hash of the last reported event associated with a filter configuration in the Windows' Isolated storage. Any changes to the filter configuration will reset the tracking information. The fields considered a part of the filter configuration are: FilterString, TimeLimitFilter, MaxMessages, RemoteMachine, EventLogName. If the last reported event has been deleted, then Radon reports all events with the same or newer timestamp. The last reported event is saved only if sending the report mail succeeds.

## Installing

You can install the Task via FRENDS UI Task view or you can find the NuGet package from the following nuget feed
`https://www.myget.org/F/frends/api/v2`

## Building

Source code is not yet available, but we hope to add it soon

## Contributing

Contributing is not possible at this time

## Technical overview

FRENDS Radon is implemented as a .NET 4 class that is run in a FRENDS4 process

### Filtering

The filter string can be used to choose which events are reported. If the string is empty, all entries will be returned.

The filter string is essentially a simple condition string a bit like SQL WHERE -clause. You define the filters using entry properties and operators (And, Or, Not, =, >, <, <> etc.). All strings need to be enclosed in double quotation marks ("). For the string-valued properties, like Message, you can call methods like Contains(), StartsWith(), EndsWith() etc.

The following table shows some examples of filter strings and what they will return.

| Filter | Returns |
|-|-|
| EntryType = "Error" | Entries of type Error (i.e. those entries showing the red ball with a cross in event viewer) |
| EntryType = "Error" Or EntryType = "Warning" | Entries of type Error or Warning |
| EntryType = "Information" | Entries of type Information. NOTE: This filter will not return any Error or Warning type entries see the next filter for that |
| ErrorLevel >= 0 | Entries of type Information, Warning or Error |
| EntryType <> "Information" | Entries of type other than Information. NOTE: This includes also those entries with no type or the SuccessAudit/FailureAudit type |
| Message.Contains("foo") | Entries whose description contains the string "foo" |
| Not Message.Contains("foo") | Entries whose description do not contain the string "foo" |
| Source <> "BizTalk Server 2009" | Entries whose source is not "BizTalk Server 2009" |
| EventID = 2345 | Entries with Event ID 2345 |
| EventID > 1000 And EventID <= 2000 | Entries with Event ID in the range of 1001 to 2000|
| (EventID > 1000 And EventID <= 2000) Or EventID = 345 | Entries with Event ID either in the range of 1001 to 2000 or 345 |

The table below shows the entry properties you can use in your queries.

| Property | Description |
|-|-|
| EntryType | The Type of the entry. Can be Error, Warning, Information, SuccessAudit or FailureAudit. Note that even though the property is an enum, you cannot compare it against integers - use the ErrorLevel property for that |
| ErrorLevel | Error level. This field is a mapping from the EntryType, and the value can be -1, 0 (Information), 1 (Warning), 2 (Error), 3 (SuccessAudit) or 4 (FailureAudit). ErrorLevel is meant for creating simple filters that return all error and warning messages (ErrorLevel >= 1), for instance. |
| Source | The Source name of the application that generated the entry, e.g. "FRENDS Helium", "MSSQLSERVER" or "BizTalk Server 2009" |
| Message | The localized message associated with the entry |
| EventID | The application specific event identifier for the entry |
| TimeGenerated | The time the entry was generated to the log. Note that you should use the TimeLimitFilter of the configuration for filtering based on time |
| CategoryNumber | The optional category number of the entry, if it has one defined |
| Category | The string describing the category of the entry, if it has the category number defined |
| InstanceID | The InstanceId property. It uniquely identifies an entry using the full 32-bit resource identifier |
| Index | The index of the entry in the event log |

### Email templates

FRENDS Radon uses the [DotLiquid](http://dotliquidmarkup.org/) library for the email templating. Both the subject and email body can be templated, the subject template is configured directly in the configuration. The body template can be provided in a file (see [templateFile](#email-templates)) if the default message needs to be modified. For the template markup syntax see the official [Liquid documentation](https://github.com/Shopify/liquid/wiki/Liquid-for-Designers).

In addition to the standard filters, we have implemented a custom regex filter: match_first, with the following syntax:

{{ 'text to match' | match_first: 'regexp'}}
The Filter returns the first regexp match for the filtered string, for example
{{ 'FRENDS Radon' | match_first: '^FRENDS (.*)$' }}
would result in: Radon
The following variables are provided to the template:

| Name | Content |
|-|-|
| events | The list of events which matched the filter. Each event has the following properties, which correspond with the fields in the event log: category_number, category, message, source, time_generated, entry_type, instance_id, index, event_id, error_level |
| count | The amount of events which matched the filter, the maximum value is limited by MaxMessages setting |
| config | The filter config with the following properties: filter_string, time_limit, max_messages, use_max_messages, remote_machine, event_log_name |
| start_time | The start of the time period during which the events were scanned.
| current_time | The local time when the report was generated. |
| machine_name | The machine name for the system whose eventlog was scanned. |
| event_log_name | The name of the eventlog which was scanned. |

The default template which is used if no custom template is defined:
```html
{% assign bgColor1 = '#f1f1f1' %}
{% assign bgColor2 = '#f2f2f2' %}

{{ count }}{% if count >= config.max_messages %} latest{% endif %} events from the {{ event_log_name }} log from {{ start_time }} to {{ current_time }} on the machine {{ machine_name }}

<table border='0' cellspacing='2px' cellpadding='2'>
    <thead>
        <tr bgcolor='{{ bgColor2 }}'>
            <th>Level</th>
            <th>Date and Time</th>
            <th>Source</th>
            <th>Event ID</th>
            <th>Category</th>
            <th>Message</th>
        </tr>
    </thead>
    <tbody>
    {% for event in events %}
        <tr bgcolor='{% cycle '#ffffff', bgcolor1 %}'>
            <td>{{ event.entry_type }}</td>
            <td>{{ event.time_generated }}</td>
            <td>{{ event.source }}</td>
            <td>{{ event.event_id }}</td>
            <td>{{ event.category }}</td>
            <td>{{ event.message | escape }}</td>
        </tr>
    {% endfor %}    
    </tbody>
</table>
<p>Filter used: <pre>{{ config.filter_string }}</pre></p>
<hr />
<p>This report was generated by FRENDS Radon ( <a href=\'http://www.frends.com\'>Frends Technology</a> )</p>
```

### External events

FRENDS Radon supports the option of providing the events from an external source. 

The event data must be in the following XML-format:

```xml
<Events>
    <Event>
        <Message>Performance counters for the WmiApRpl (WmiApRpl) service were loaded successfully. The Record Data in the data section contains the new index values assigned to this service.</Message>
        <Source>Microsoft-Windows-LoadPerf</Source>
        <TimeGenerated>2014-10-14 13:59:09Z</TimeGenerated>
        <Category>(0)</Category>
        <EventID>1000</EventID>
        <CategoryNumber>0</CategoryNumber>
        <Index>197340</Index>
        <ErrorLevel>0</ErrorLevel>
        <InstanceID>1000</InstanceID>
        <EntryType>Information</EntryType>
    </Event>
    <Event>
        <Message>The description for Event ID '1073742831' in Source 'Customer Experience Improvement Program' cannot be found.  The local computer may not have the necessary registry information or message DLL files to display the message, or you may not have permission to access them.  The following information is part of the event:</Message>
        <Source>Customer Experience Improvement Program</Source>
        <TimeGenerated>2014-10-14 13:50:08Z</TimeGenerated>
        <Category>(0)</Category>
        <EventID>1007</EventID>
        <CategoryNumber>0</CategoryNumber>
        <Index>197338</Index>
        <ErrorLevel>0</ErrorLevel>
        <InstanceID>1073742831</InstanceID>
        <EntryType>Information</EntryType>
    </Event>
</Events>
```

When using external events, FRENDS Radon does not enforce the time limit 'maxTimespan' for the events, although previously reported events are still filtered out. This means that the TimeGenerated needs to be set to a non-null value

## Parameter reference

### Frends.Radon.Execute

The main Radon task

Email:

| Property                | Type   | Description                                                                                 |
| ----------------------- | ------ | ------------------------------------------------------------------------------------------- |
| Username                | string | SMTP username for sending email. As noted above, can be left blank if you are using Windows credentials or anonymous authentication                                       |
| Use windows credentials | bool   | If the SMTP server requires authentication, you can use the Windows credentials of the account running the Radon executable by setting this parameter value to 'True'. On the other hand, if the server does not require authentication, you can send emails anonymously by setting this parameter value to 'False' and leaving Username parameter empty |
| Subject                 | string | Text to be displayed on email subject line. The accepts the same template format as the TemplateFile if more complex subjects are needed                                                                          |
| Smtp server name        | string | Server name or IP address of host to be used for SMTP actions                               |
| Sender name             | string | Display name shown as the sender name in emails                                                                           |
| Sender address          | string | Email address to be used with sent emails. NOTE: Some SMTP servers filter the sent messages based on the sender address, so make sure your server accepts this address                                                                        |
| Recipients              | string | Email recipients, separated with a semicolon (;)                                            |
| Port number             | int    | Port number for SMTP actions. Should be 25 by default                                       |
| Password                | string | Password for SMTP user. Must be given if the Username is given - you cannot use an empty password for username/password authentication NOTE: Password will be sent in clear text to the server.                                    |
| Max idle time           | int    | Sets up a time (in milliseconds), enclosed in value-tags, how long connection can remain idle before it is closed                                                             |
| Template file           | string | The path to the file which contains the Liquid template to use. The default template will be used if left empty. See [Template](#template) for the template format                                        |
| Use ssl                 | string | Should the connection to the SMTP server use SSL                                            |

Filter:

| Property            | Type     | Description                                                                                                                     |
| ------------------- | -------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Filter string       | string   | The filter used when reading the event log, see information about [filtering](#filtering)                                    |
| Max timespan        | TimeSpan | From how long from the past we want to report entries? This setting should match the schedule, i.e. if you send the report every hour you should set the time limit to 1:00:00, or one hour. One hour by default. NOTE: This value is a TimeSpan, which can be created with an expression, e.g. TimeSpan.FromMinutes(60)                                                                                               |
| Event source        | Enum     | Should the event log be used as the source of events or an external XML structure provided in the External events xml parameter |
| External events xml | string   | XML used as the event source, see [external events](#external-events)                                                           |
| Max messages        | int      | The maximum number of entries to include in a report. 200 by default. To return all possible entries, set this to 0                                                                                    |
| Remote machine      | string   | The name of the machine whose event log will be read. Leave this empty if the target machine is the local machine. NOTE: Reading a remote event log is much slower than reading a local event log. In addition, the user account with which FRENDS Radon is run must be able to read the event log. To allow this, the user account needs to be added to the target computer's Event Log Readers group                                                      |
| Event log name      | string   | The name of the event log which will be read, e.g. Application or Security. Defaults to Application if left empty                                                                       |

### Frends.Radon.SendMail

The FRENDS Radon mailing functionality is provided as the Task 'FRENDS Radon SendMail', which can be used to send arbitrary email messages. The Mailer task uses the same email settings as the FRENDS Radon except it does not support the email templates. Instead of the template, it has the 'MessageContent' property which can be used to set the email message body.

Email:

| Property                | Type   | Description                                                                                 |
| ----------------------- | ------ | ------------------------------------------------------------------------------------------- |
| Username                | string | SMTP username for sending email. As noted above, can be left blank if you are using Windows credentials or anonymous authentication                                       |
| Use windows credentials | bool   | If the SMTP server requires authentication, you can use the Windows credentials of the account running the Radon executable by setting this parameter value to 'True'. On the other hand, if the server does not require authentication, you can send emails anonymously by setting this parameter value to 'False' and leaving Username parameter empty |
| Subject                 | string | Text to be displayed on email subject line. The accepts the same template format as the TemplateFile if more complex subjects are needed                                                                          |
| Smtp server name        | string | Server name or IP address of host to be used for SMTP actions                               |
| Sender name             | string | Display name shown as the sender name in emails                                                                           |
| Sender address          | string | Email address to be used with sent emails. NOTE: Some SMTP servers filter the sent messages based on the sender address, so make sure your server accepts this address                                                                        |
| Recipients              | string | Email recipients, separated with a semicolon (;)                                            |
| Port number             | int    | Port number for SMTP actions. Should be 25 by default                                       |
| Password                | string | Password for SMTP user. Must be given if the Username is given - you cannot use an empty password for username/password authentication NOTE: Password will be sent in clear text to the server.                                    |
| Max idle time           | int    | Sets up a time (in milliseconds), enclosed in value-tags, how long connection can remain idle before it is closed                                                             |
| Use ssl                 | string | Should the connection to the SMTP server use SSL                                            |
| Message Content         | string | The content of email message                                                |
