# <img alt="logo" height="26" src="https://github.com/mob-sakai/mob-sakai/assets/12690315/1cc2c0f3-32bf-4635-a27e-d3f906aaf9ab"/> UI Material Property Injector

[![](https://img.shields.io/npm/v/com.coffee.ui-material-property-injector?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.coffee.ui-material-property-injector/)
[![](https://img.shields.io/github/v/release/mob-sakai/UIMaterialPropertyInjector?include_prereleases)](https://github.com/mob-sakai/UIMaterialPropertyInjector/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/UIMaterialPropertyInjector.svg)](https://github.com/mob-sakai/UIMaterialPropertyInjector/releases)  
![](https://img.shields.io/badge/Unity-2019.4+-57b9d3.svg?style=flat&logo=unity)
![](https://img.shields.io/badge/uGUI_2.0_Ready-57b9d3.svg?style=flat)
[![](https://img.shields.io/github/license/mob-sakai/UIMaterialPropertyInjector.svg)](https://github.com/mob-sakai/UIMaterialPropertyInjector/blob/main/LICENSE.md)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)
[![](https://img.shields.io/github/watchers/mob-sakai/UIMaterialPropertyInjector.svg?style=social&label=Watch)](https://github.com/mob-sakai/UIMaterialPropertyInjector/subscription)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)

<< [🎮 Demo](#-demo) | [⚙ Installation](#-installation) | [🚀 Usage](#-usage) | [🤝 Contributing](#-contributing) >>

<br><br>

## 📝 Description

In Unity UI, UI elements typically do not provide an accessible MaterialPropertyBlock. To change material properties via animations, like with MeshRenderer, you usually need to create custom components, which are often shader-specific.

Custom properties for changing material properties are easy for experienced users but can be a high hurdle for beginners:
- Are material assets being changed directly?
- Are material instances leaking?
- Is it allocating unnecessarily every time the property changes?
- Does it support masks?
- Does it support animations?
- And so on.

This package provides a component that allows easy modification of material properties for Unity UI (uGUI) without the need for shader-specific custom components.

![](https://github.com/mob-sakai/UIMaterialPropertyInjector/assets/12690315/adaf45ed-5852-4844-ad1a-daa231225be7)

**Key Features:**

- Change UI material properties without shader-specific custom components.
- Modify material properties:
  - via animation, similar to `MeshRenderer`
  - via [tweener](#ui-material-property-tweener) component
  - via code
  - via the inspector
- Automatic creation and deletion of material instances.
- Instead of creating material assets with slight differences, you can create material instances and change properties.
- Share material instances by specifying a `GroupId`.
- Automatic detection of shader properties.
- Supports the `Mask` component.
- Supports `TextMeshProUGUI` and `SubMeshUI` components. (uGUI 2.0 ready)
- Good performance.

![](https://github.com/mob-sakai/mob-sakai/assets/12690315/1f0fdd4d-83d6-4819-93ec-31c71f9e89d3)
![](https://github.com/mob-sakai/mob-sakai/assets/12690315/ffde82f5-58da-4793-848f-78e53aebad88)

<br><br>

## 🎮 Demo

[WebGL Demo](http://mob-sakai.github.io/UIMaterialPropertyInjector/)

<br><br>

## ⚙ Installation

_This package requires **Unity 2019.4 or later**._

#### Install via OpenUPM

- This package is available on [OpenUPM](https://openupm.com) package registry.
- This is the preferred method of installation, as you can easily receive updates as they're released.
- If you have [openupm-cli](https://github.com/openupm/openupm-cli) installed, then run the following command in your project's directory:
  ```
  openupm add com.coffee.ui-material-property-injector
  ```
- To update the package, use Package Manager UI (`Window > Package Manager`) or run the following command with `@{version}`:
  ```
  openupm add com.coffee.ui-material-property-injector@1.0.0
  ```

#### Install via UPM (with Package Manager UI)

- Click `Window > Package Manager` to open Package Manager UI.
- Click `+ > Add package from git URL...` and input the repository URL: `https://github.com/mob-sakai/UIMaterialPropertyInjector.git?path=Packages/src`  
  ![](https://gist.github.com/assets/12690315/24af63ed-8a2e-483d-9023-7aa53d913330)
- To update the package, change suffix `#{version}` to the target version.
   - e.g. `https://github.com/mob-sakai/UIMaterialPropertyInjector.git?path=Packages/src#1.0.0`

#### Install via UPM (Manually)

- Open the `Packages/manifest.json` file in your project. Then add this package somewhere in the `dependencies` block:
  ```json
  {
    "dependencies": {
      "com.coffee.ui-material-property-injector": "https://github.com/mob-sakai/UIMaterialPropertyInjector.git?path=Packages/src",
      ...
    }
  }
  ```

- To update the package, change suffix `#{version}` to the target version.
   - e.g. `"com.coffee.ui-material-property-injector": "https://github.com/mob-sakai/UIMaterialPropertyInjector.git?path=Packages/src#1.0.0",`

#### Install as Embedded Package

1. Download a source code zip file from [Releases](https://github.com/mob-sakai/UIMaterialPropertyInjector.git/releases) and extract it.
2. Place it in your project's `Packages` directory.  
   ![](https://github.com/mob-sakai/mob-sakai/assets/12690315/0b7484b4-5fca-43b0-a9ef-e5dbd99bcdb4)
- If you want to fix bugs or add features, install it as an embedded package.
- To update the package, you need to re-download it and replace the contents.

<br><br>

## 🚀 Usage

1. Add the `UIMaterialPropertyInjector` component to a graphic (Image, RawImage, Text, etc.) and mark the shader properties as injectable.  
   ![](https://github.com/mob-sakai/mob-sakai/assets/12690315/1f0fdd4d-83d6-4819-93ec-31c71f9e89d3)
2. Change the properties via animation, code or tweener.
   ![](https://github.com/mob-sakai/mob-sakai/assets/12690315/ffde82f5-58da-4793-848f-78e53aebad88)
   ```csharp
   var injector = GetComponent<UIMaterialPropertyInjector>();
   injector.SetFloat("_Intensity", 0.9f);
   ```
3. Enjoy!

<br><br>

#### UI Material Property Injector

Change the material properties of the CanvasRenderer.

![](https://github.com/mob-sakai/mob-sakai/assets/12690315/733254f3-9062-460c-ae88-2e01a080f072)

- **Reset Values On Enable:** Reset injector values with the material properties when the component is enabled.
- **Animatable:** Makes it animatable in the Animation view.
- **Shared Group Id:** Share material instances by specifying a `GroupId`.
  - NOTE: The material instances cannot be shared if the mask depth is different.
- **Properties:** Shader properties to inject.
  - Click `Reset Values` to reset injector values to the material properties. 

<br><br>

#### UI Material Property Tweener

A tweener to change the material properties.

![](https://github.com/mob-sakai/mob-sakai/assets/12690315/82676be2-0f98-4bb4-bc1d-203a70b85a85)

- **Target:** The target `UIMaterialPropertyInjector` to tween.
- **Curve:** The curve to tween the properties.
- **Delay:** The delay in seconds before the tween starts.
- **Duration:** The duration in seconds of the tween.
- **Interval:** The interval in seconds between each loop.
- **Restart On Enable:** Whether to restart the tween when enabled.
- **Wrap Mode:** The wrap mode of the tween.
  - `Clamp`: Clamp the tween value, not loop.
  - `Loop`: Loop the tween value.
  - `PingPongOnce`: PingPong the tween value, not loop.
  - `PingPong`: PingPong the tween value.
- **Update Mode:** Specifies how to get delta time.
  - `Normal`: Use `Time.deltaTime`.
  - `Unscaled`: Use `Time.unscaledDeltaTime`.
  - `Manual`: Not updated automatically and update manually with `UpdateTime` or `SetTime` method.
- **Properties:** Shader properties to inject. The tweener interpolates two values with an animation curve.
  - Click `Reset Values` to reset injector values to the material properties.
<br><br>
    
## 🤝 Contributing

### Issues

Issues are incredibly valuable to this project:

- Ideas provide a valuable source of contributions that others can make.
- Problems help identify areas where this project needs improvement.
- Questions indicate where contributors can enhance the user experience.

### Pull Requests

Pull requests offer a fantastic way to contribute your ideas to this repository.  
Please refer to [CONTRIBUTING.md](https://github.com/mob-sakai/UIMaterialPropertyInjector/tree/main/CONTRIBUTING.md)
and [develop branch](https://github.com/mob-sakai/UIMaterialPropertyInjector/tree/develop) for guidelines.

### Support

This is an open-source project developed during my spare time.  
If you appreciate it, consider supporting me.  
Your support allows me to dedicate more time to development. 😊

[![](https://user-images.githubusercontent.com/12690315/50731629-3b18b480-11ad-11e9-8fad-4b13f27969c1.png)](https://www.patreon.com/join/2343451?)  
[![](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)

<br><br>

## License

* MIT

## Author

* ![](https://user-images.githubusercontent.com/12690315/96986908-434a0b80-155d-11eb-8275-85138ab90afa.png) [mob-sakai](https://github.com/mob-sakai) [![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai) ![GitHub followers](https://img.shields.io/github/followers/mob-sakai?style=social)

## See Also

* GitHub page : https://github.com/mob-sakai/UIMaterialPropertyInjector
* Releases : https://github.com/mob-sakai/UIMaterialPropertyInjector/releases
* Issue tracker : https://github.com/mob-sakai/UIMaterialPropertyInjector/issues
* Change log : https://github.com/mob-sakai/UIMaterialPropertyInjector/blob/main/CHANGELOG.md
