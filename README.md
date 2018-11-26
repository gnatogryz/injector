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
