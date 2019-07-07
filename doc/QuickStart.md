Quick Start
===========

Setting up a hook using SpyGlass involves the following steps:

1. Open the process:
    ```csharp
    var remoteProcess = new RemoteProcess(processId);
    ```

2. Inject the library using e.g. the LoadLibrary injection method:
    ```csharp
    var injector = new LoadLibraryInjector();
    injector.InjectDll(remoteProcess, dllPath);
    ```

3. Set up a hooking session:
    ```csharp
    _hookSession = new HookSession(remoteProcess, new AsmResolverParametersDetector());
    _hookSession.HookTriggered += HookSessionOnHookTriggered;
    _hookSession.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));
    ```

4. Set a hook at the provided addess:
    ```csharp
    _hookSession.Set(address);
    ```

5. Use the callbacks:
    ```csharp
    private static void HookSessionOnHookTriggered(object sender, HookEventArgs e)
    {
        // Go ham here.
    }
    ```

6. ???
7. Profit
