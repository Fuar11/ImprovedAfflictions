# Improved Afflictions

Improved Afflictions is a gameplay mod for **The Long Dark** by Hinterland Studios

Improved Afflictions aims to increase the challenge and immersion of The Long Dark by tweaking, improving and adding new afflictions to the game.

## New Afflictions

Among many tweaked and changes afflictions, IA adds a collection of new and improved afflictions that represent the long term effects of affliction & injury pain by overriding the pain affliction. All of these afflictions cause pain themselves, but typically last longer than the standard pain affliction. Below is a list.

### Animal Bites

A bite from a wolf or bear during an attack can cause blood loss or a sprain. Upon receiving either of those afflictions, the wolf bite or bear bite affliction will be contracted alongside. These bites can last upwards of 5 days.

### Falls

Falling from a rope, heights or simply tripping on an incline or slope can cause a sprained ankle or sprained wrist. While you can stabilize the afflicted area (vanilla affliction), the pain will last for a couple more (up to 5) days. This affliction is most similar to vanilla pain, but lasts much longer.

### Concussion

Blunt trauma to the head may result in a concussion, a new affliction that lasts a long time and causes blurred and tunnel vision. One can contract head trauma from falling off ropes or being attacked by a moose or a bear. Concussions can last up to 10 days and are not easy to deal with. But a good way to prevent a concussion is by wearing a protective piece of headgear, like the miner's hardhat which can reduce the risk of a concussion by 50%!

### Chemical Burns

When exposed to toxic chemicals, the risk of chemical poisoning will increase much quicker than in vanilla. Alongside this, walking through corrosive fumes with exposed hands or feet will result in getting burned on those exposed very quickly. Corrosive chemical burns will be applied to both hands and feet and last up to 5 to 10 days depending on severity and the area. 

## Pain

While the pain affliction has been repurposed, these new afflictions are not without pain. Each affliction now has it's own pain level. When an affliction is contracted, the pain level is at it's maximum and will go down over time as the wound heals. Various afflictions have differing levels of pain, but pain will always go down at a consistent rate per affliction. Once the affliction has healed enough and it's pain has reduced to 30% of it's maximum, the effects will be lessened.

The effects of pain depend on the severity, affliction and where it's located. But here's a list of what to expect:

* Blurred and distorted visual effects
* Tunnel vision for concussions and intense pain pulses
* Slower walk speed for lower body injuries on legs and feet
* Slower break down and crafting speed for upper body injuries on arms and hands
* Climbing restriction on ropes and rocks (two hands, hand and arm, both arms, combination of those 3, etc...)
* Slower climbing speed if you are able to climb
* Reading restriction if you have a concussion
* Etc...

## Painkillers

The effects of pain do not go away quickly, your injuries take time to heal. Unlike in vanilla, painkillers will not instantly heal any pain you have. There are no quick fixes. 
Painkillers are a way to temporarily ease the effects of pain. You can take painkillers at any time, even if you have no afflictions. When you take 1 dose (2 pills) of painkillers, it takes 20 minutes in game time to kick in. The new Blood Drug Level counter in your status screen is a UI element that represents how much painkillers you have in your system. Like pain, they will go down over time. It takes 10 hours in game for it to leave your system. The more doses you take, the more the half-life time increases relative to how much you've taken. Additionally, the amount of drugs a dose of painkillers gives is relative to the condition of the meds.

Painkillers will reduce the effects of pain by varying degrees. If your blood drug level is higher than the pain level of an affliction, the painkillers are taking effect. Experiment with this! 

## Emergency Stim

Emergency Stims also act as a painkiller. But instead of taking 20 minutes to kick in, they give an instant 50% administration of drugs into your system. Useful in a pinch!

## Overdose

Painkillers are not without their downsides. Taking too much can result in an overdose! More than 60% blood drug level and you will slowly get tired quicker. The more you take, the more this will worsen. If your blood drug level is higher than 80%, nearing 100%, you will be overdosing. While overdosing, you will get tired significantly quicker and begin to lose condition. As well as stumble around as you are high on the opioids running through your system. 

Overdose will be fleshed out as feedback comes in.

## Food Poisoning, Dysentery, Hypothermia and Scurvy

These afflictions are not new but have had some tweaks and changes done to make them more of a challenge and a threat.

### Food Poisoning & Dysentery

Food poisoning & dysentery are now more common, more deadly and take longer to recover from. 

Low quality food has a much higher chance of making you ill, the type of food will dictate what disease you contract. Generally, wet foods will contain dysentery causing bacteria. Including sodas.
If the food makes you sick, it will take some time for the effects of the disease to kick in so you won't notice until much later. These values can be adjusted in the Mod Settings menu and can be set to 0 to turn it off for instant food poisoning.

Both illnesses will drain you, weakaning you until you die. Resting is still the best way to recover but both diseases will heal over time on their own. However taking antibiotics will reduce their
impact on your health. Condition loss isn't stunted when sleeping, although it is dimished, especially while on antibiotics. 

Food poisoning contraction chance is based on the condition of the food. If a food item is below 45% you have a chance of getting food poisoning. Any higher and you are safe. The chance is directly proportional to the condition, so 25% condition food will give a 75% chance of getting food poisoning. On difficulties such as interloper, or custom where the decay rate is very high, there are optional settings to reduce the chance, offset by 20%. So 25% condition will actually be 55% instead of 75%. This is to ensure players can still consume man-made food early on. There is also a setting to reduce the condition treshold from 45% to 25%. 

The amount of condition you lose while sick from food poisoning will depends on your other stats, including number of injuries.

### Hypothermia

While under the cold grip of hypothermia, you will feel colder all around and warming up will take slightly more time.

### Scurvy

Scurvy is now more deadly. A low vitamin c diet has resulted in any vitamin c devoid foods being far less nutritious. Foods without vitamin c or low vitamin c will give 50% less calories. In addition,
you will get tired and colder much quicker. Along with a slight increase the carry capacity penalty. 


## Installation

* Install MelonLoader by downloading and running [MelonLoader.Installer.exe](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe)
* Install the following dependencies in your mods folder: 

- [AfflictionComponent](https://github.com/TLD-Mods/AfflictionComponent/releases/latest)
- [ModData](https://github.com/dommrogers/ModData/releases/latest)
- [Moment](https://github.com/No3371/TLD-Moment/releases/latest)
- [ModSettings](https://github.com/DigitalzombieTLD/ModSettings/releases/latest)
- [ComplexLogger](https://github.com/Arkhorse/Complex-Logger/releases/latest)

* Install the latest release and drop it in your mods folder
