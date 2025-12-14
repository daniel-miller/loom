# README

## IIS Express Configuration

When running locally in IIS Express, you need to allow the `ORGANIZATION_SLUG` server variable for URL Rewrite to work.

1. Open `.vs\{SolutionName}\config\applicationhost.config` (the `.vs` folder is hidden)

2. Find the `<rewrite>` section inside `<system.webServer>`

3. Add the `<allowedServerVariables>` block:

```xml
<rewrite>
    <allowedServerVariables>
        <add name="ORGANIZATION_SLUG" />
    </allowedServerVariables>
    <!-- existing rules -->
</rewrite>
```

4. Restart IIS Express completely (exit from system tray or kill `iisexpress.exe` in Task Manager)

Without this change, you'll see: `HTTP Error 500.50 - URL Rewrite Module Error. The server variable "ORGANIZATION_SLUG" is not allowed to be set.`

## IIS Configuration

When running in IIS, you need the same change to `%windir%\system32\inetsrv\config\applicationHost.config` or you will see the same run-time error.