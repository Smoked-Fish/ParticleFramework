### Content Patcher Example
```
{
	"Format": "2.0.0",
	"Changes": [
		{
			"Action": "EditData",
			"Target": "Mods/Espy.ParticleFramework/dict",
			"Entries": {
				"Espy.CPParticleFramework/darkThronePuffer": {
					"type":"furniture",
					"name":"(F)64",
					"movementType":"random",
					"movementSpeed": 2,
					"frameSpeed": 0.1,
					"acceleration":0,
					"minRotationRate":0.1,
					"maxRotationRate":0.1,
					"particleWidth":57,
					"particleHeight":58,
					"fieldInnerRadius":64,
					"fieldOuterRadius":128,
					"fieldOffsetX": 16,
					"fieldOffsetY": -32,
					"minParticleScale": 0.2,
					"maxParticleScale": 1,
					"maxParticles": 30,
					"minLifespan": 40,
					"maxLifespan": 80,
					"spriteSheetPath": "Mods/Espy.ParticleFramework/pufferchick"
				}
			}
		},
		{
			"Action": "Load",
			"Target": "Mods/Espy.ParticleFramework/pufferchick",
			"FromFile": "assets/pufferchick.png"
		}
	]
}
```


### C# Example
```c#

private IParticleFrameworkApi _particleFrameworkApi;
private ParticleEffectData particleEffectData;

private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
{
        _particleFrameworkApi = Helper.ModRegistry.GetApi<IParticleFrameworkApi>("Espy.ParticleFramework");
        particleEffectData = new ParticleEffectData()
        {
                key = "Espy.CPParticleFramework/darkThronePuffer",
                type = "furniture",
                name = "(F)64",
                movementType = "random",
                movementSpeed = 2,
                frameSpeed = 0.1f,
                acceleration = 0,
                minRotationRate = 0.1f,
                maxRotationRate = 0.1f,
                particleWidth = 57,
                particleHeight = 58,
                fieldInnerRadius = 64,
                fieldOuterRadius = 128,
                fieldOffsetX = 16,
                fieldOffsetY = -32,
                minParticleScale = 0.2f,
                maxParticleScale = 1,
                maxParticles = 30,
                minLifespan = 40,
                maxLifespan = 80,
                spriteSheetPath = "assets/pufferchick.png"
        };
        _particleFrameworkApi.LoadEffect(particleEffectData);
}


```
