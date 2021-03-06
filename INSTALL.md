Installing .NET Warden Service
---------------------------------
* Operating Systems:
  * Tested on Windows 8 and Server 2012
* Prerequisites:
  * .NET Framework 4.5
  * Install *IIS* (ensure that the *.NET Extensibility* feature is enabled)
  * Install *IIS Hostable Web Core*
  * Ensure `Administrators` group owns `C:\IronFoundry` and has `Full Control`. The Ruby DEA runs as `Local Service` so ensure this user has full control as well on this directory. Leave the default inherited permissions intact.
  * Create a dedicated user in `Administrators` group with which to run the Warden service. An admin user is required due to the fact that the Warden service creates unprivileged user accounts for containers.
    `NT AUTHORITY\Local Service` does not have these permissions. `Local System` can not be used due to the fact that the service uses the `CreateProcessWithLogonW` API call to run subprocesses.
  * Set `powershell` execution policy to `RemoteSigned`. Don't forget to also set 32-bit powershell here `C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe` (may not be required)
    	* Must be run as an Administrator:
    	
    	```
    	PS> Set-ExecutionPolicy RemoteSigned
    	```

Be sure to update your Cloud Controller's `config/stacks.yml` to recognize the `windows2012` stack:

```
vagrant@precise64:/vagrant$ cat cloud_controller_ng/config/stacks.yml 
default: lucid64

stacks:
  - name: lucid64
    description: Ubuntu Lucid 64-bit
  - name: windows2012
    description: Microsoft .NET / Windows 64-bit
```

To install the warden as a service, use the following commmands:

```IronFoundry.Warden.Service.exe install -username:<ifuser> -password:<password> --autostart```

You can also start and stop the service from the command line by using:

```IronFoundry.Warden.Service.exe stop```

```IronFoundry.Warden.Service.exe start```

You can uninstall the service via the uninstall command:

```IronFoundry.Warden.Service.exe uninstall```
