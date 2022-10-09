# Thread Dispatcher
Thread Dispatcher is an open source tool to pass the execution of a Delegate, Coroutine or Task from a background thread to the main thread, and await its completion or result on the calling thread as needed.

&nbsp;
## Installation and Updates

### Option 1. **Install via Open UPM (recommended)** [![openupm](https://img.shields.io/npm/v/com.baracuda.thread-dispatcher?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.baracuda.thread-dispatcher/)

- open <kbd>Edit/Project Settings/Package Manager</kbd>
- add a new Scoped Registry:

    - Name: OpenUPM
    - URL:  https://package.openupm.com
    - Scope(s): com.baracuda

- click <kbd>Save</kbd>
- open <kbd>Window/Package Manager</kbd>
- click <kbd>+</kbd>
- click <kbd>Add package by name...</kbd>
- paste and <kbd>Add</kbd>  `com.baracuda.thread-dispatcher`
- take a look at [Setup](#customized-setup) to see what comes next

#### Option 2. Install via Git URL

- open <kbd>Window/Package Manager</kbd>
- click <kbd>+</kbd>
- click <kbd>Add package from git URL</kbd>
- paste and <kbd>Add</kbd> `https://github.com/JohnBaracuda/com.baracuda.thread-dispatcher.git`
- take a look at [Setup](#customized-setup) to see what comes next

#### Option 3. Get Thread Dispatcher from the [Asset Store](https://assetstore.unity.com/packages/slug/202421)


#### Option 4. Download a <kbd>.unitypackage</kbd> from [Releases](https://github.com/JohnBaracuda/com.baracuda.thread-dispatcher/releases)

&nbsp;

> If you like thread dispatcher, consider leaving a good review on the Asset Store regardless of which installation method you chose :)

&nbsp;

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
