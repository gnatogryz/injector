# What is this

This is a convenience thing to make things convenient. A lazy-init function + injector for Unity serialized fields.

## How to do things

Lazy init example of a camera component:

```c#
Camera _cam;
public Camera camera => this.Lazy(ref _cam);
```

Inject a component from hierarchy (editor-only):

```c#
[FindInChildrenByName]
public UnityEngine.UI.Button acceptButton;
[FindInChildrenByName("acceptButton")]
public UnityEngine.UI.Image acceptButtonImage;
```

The injection occurs on script reload, scene save etc. **Not in runtime**.
The injection is verbose, which means you'll get feedback on found/missing components.

See it do stuff:

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/at9qnACb3tU/0.jpg)](https://www.youtube.com/watch?v=at9qnACb3tU)