# Thread Dispatcher
Thread Dispatcher is an open source tool to pass the execution of a Delegate, Coroutine or Task from a background thread to the main thread, and await its completion or result on the calling thread as needed.

## Installation

- Option 1. UPM git URL `https://github.com/JohnBaracuda/com.baracuda.thread-dispatcher.git`
    <details>
    <summary>More details how to add a git URL with the package manager.</summary>

   - open <kbd>Window/Package Manager</kbd>
   - click <kbd>+</kbd>
   - click <kbd>Add package from git URL</kbd>
   - paste `https://github.com/JohnBaracuda/com.baracuda.thread-dispatcher.git`
   - click <kbd>Add</kbd>
    </details>

   
- Option 2. Download <kbd>.unitypackage</kbd> from [Releases](https://github.com/JohnBaracuda/com.baracuda.thread-dispatcher/releases)  
   
- Option 3. Download from [Asset Store](https://assetstore.unity.com/packages/slug/202421)  


## How to use

• [Detailed Documentation](https://johnbaracuda.com/dispatcher.html)

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
