## [1.2.1](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.2.0...1.2.1) (2026-06-12)


### Bug Fixes

* compile error in Unity 2019.4 and 2020.x ([148ea1a](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/148ea1ab7082aa05c34efa435bc9127e581c3fab))
* support Unity 6.5 ([45817ac](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/45817ac7160b8cae98a11bc33ac3cb79e7b84f0b))

# [1.2.0](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.1.3...1.2.0) (2025-09-12)


### Bug Fixes

* generic mode ([e216b16](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/e216b1655e27f73af4b1ae7a1c2237bb90473ef1))
* injected material not generated when injection process runs without getter and setter configured ([210f46c](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/210f46c2de915d17131f84280fcfdd89fecddd2e))


### Features

* add `renderer` and `material` properties for `RendererMaterialPropertyInjector` ([cb5c919](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/cb5c919dcde8a558db9d514054a74bfe2548c4b8))
* add invalid material accessor warning in inspector ([33305db](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/33305db1fcdac77184814a91f453455f7e4a4781))
* generic mode ([b34dbc7](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/b34dbc7feac0da4cba6f52b2141238e00d1458b1))
* mutual synchronization system between base-material and injected-material (editor) ([c1ae500](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/c1ae5006dfacaa1c470d032ed4e2404720b4d4b5))
* renderer mode ([b70c644](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/b70c64441cc85d549270065d4f5907e83cda113d))

## [1.1.3](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.1.2...1.1.3) (2025-06-22)


### Bug Fixes

* ambiguous reference errors occur between `Coffee.UIExtensions.Color` and `UnityEngine.Color` ([24160fd](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/24160fd0ce415ade2f24bc13d07248370089aebd)), closes [#11](https://github.com/mob-sakai/UIMaterialPropertyInjector/issues/11)

## [1.1.2](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.1.1...1.1.2) (2025-02-17)


### Bug Fixes

* properties '_UIMaskSoftnessX/Y' are common property ([ee34e1b](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/ee34e1b9099a1ccb83146a15f89c73e339f9fcc1))
* properties with attributes prefixed with 'Material' or suffixed with 'Drawer' are not displayed correctly ([e5b92fe](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/e5b92fe8ebc3d24646988e2d4f7295dd6d80308e))
* properties with non-keyword toggle attributes are not displayed in the list ([46d58aa](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/46d58aa10e34e6a0e21d80ae2bc71307723c901d)), closes [#8](https://github.com/mob-sakai/UIMaterialPropertyInjector/issues/8)

## [1.1.1](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.1.0...1.1.1) (2025-02-04)


### Bug Fixes

* tweener properties are not rendered correctly in the Inspector in the editor. ([ba22663](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/ba2266318b8497efdee4fd6c1c3821c14ebc58a3)), closes [#7](https://github.com/mob-sakai/UIMaterialPropertyInjector/issues/7)

# [1.1.0](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.0.4...1.1.0) (2025-01-30)


### Bug Fixes

* ignore properties with `Toggle`, `KeywordEnum` and `PerRendererData` attributes ([343d9e5](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/343d9e5a51a396fe083fb0b41f2cd3cd18dc5afa))


### Features

* support `MaterialToggle`, `Enum`, `IntSlider` and `PowerSlider` shader property attributes ([9881112](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/988111243dbe81d9f19af9cc87136b9c02ba631b))
* use a searchable dropdown instead of a popup for property selection ([ca62a19](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/ca62a19627d5c9ff4be9093a0745feba1df349de)), closes [#6](https://github.com/mob-sakai/UIMaterialPropertyInjector/issues/6)

## [1.0.4](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.0.3...1.0.4) (2025-01-28)


### Bug Fixes

* vector4 properties such as `_MainTex_ST` cannot be modified from the Inspector in editor ([5805fd8](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/5805fd8ec5f0b0167d7dcda2ca5f378c4fa26506))

## [1.0.3](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.0.2...1.0.3) (2024-10-02)


### Bug Fixes

* make several APIs virtual for inheritance purposes. ([a6d57d2](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/a6d57d27fb69ecd793c5ba8092eb1c1784f8d92b))
* property changes by AnimationClip are not applied on the frame where the GameObject is activated in the editor ([35d9d20](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/35d9d2035e39e0ad3991947530698015fb10ac8f))
* when pressing the "add" button in the Tweener inspector, an error occurs. ([c661bcd](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/c661bcdb3b74f16f6df4894c0f26951c2162c345))

## [1.0.2](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.0.1...1.0.2) (2024-07-03)


### Bug Fixes

* nest less-frequently used properties within dropdown (editor) ([ad67744](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/ad67744c0525a1c3f2be7dcc7ccea94084d2b914))

## [1.0.1](https://github.com/mob-sakai/UIMaterialPropertyInjector/compare/1.0.0...1.0.1) (2024-07-01)


### Bug Fixes

* material properties do not change when Mask is disabled ([5bd37cd](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/5bd37cd41f8500417d717484d8439667e5cae0a1)), closes [#1](https://github.com/mob-sakai/UIMaterialPropertyInjector/issues/1)
* the background color of the inspector does not change while previewing or recording in the animation view ([6ea719f](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/6ea719f13270f08bb6e467831f16096f6bbd88a6)), closes [#2](https://github.com/mob-sakai/UIMaterialPropertyInjector/issues/2)

# 1.0.0 (2024-06-27)


### Features

* first release ([c6c8985](https://github.com/mob-sakai/UIMaterialPropertyInjector/commit/c6c8985857e3be807e8e392120925b1e036c094d))
