# Thread Dispatcher
Thread Dispatcher is an open source tool to pass the execution of a Delegate, Coroutine or Task from a background thread to the main thread, and await its completion or result on the calling thread as needed.


• [Detailed Documentation](https://johnbaracuda.com/dispatcher.html)  
• [Asset Store](https://assetstore.unity.com/packages/slug/202421)  


### Example
```c#
// Task is running on a background thread.
public async Task  WorkerTask()  
{  
    // Dispatch an Action that is executed on the main thread.  
    Dispatcher.Invoke(() =>  
    {
	// Executed on main thread.
    });  

    // Dispatch an Action that is executed on the main thread and await its completion.  
    await Dispatcher.InvokeAsync(() =>  
    {  
	// Executed on main thread.
    });  

    // Dispatch a Func<TResult> that is executed on the main thread and await its result.  
    var player = await Dispatcher.InvokeAsync(() =>  
    {  
	// Executed on main thread.
	return  FindObjectOfType<Player>();
    }); 
}
```

### Features
• Dispatch the execution of an Action to the main thread.  
• Dispatch the execution of a **Func<TResult>** to the main thread.  
• Dispatch the execution of a **Coroutine** to the main thread.  
• Dispatch the execution of a **Task** to the main thread.  
• Dispatch the execution of a **Task<TResult>** to the main thread.  
• **Await** the **execution & result** of a delegate or task on the calling thread.  
• **Await** the **start** or the **competion** of a Coroutine on the calling thread.  
• Asynchronous overloads have **full cancellation support**.  
• Multiple **extension methods** to reduce boilder plate code.  
• Full C# **source code** included.  
 
 
❤️❤️❤️ [Donations | PayPal.me](https://www.paypal.com/paypalme/johnbaracuda) ❤️❤️❤️
