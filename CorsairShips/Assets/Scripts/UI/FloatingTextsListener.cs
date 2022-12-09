using UTPLib.Services;

namespace UI.FloatingTexts {
	public class FloatingTextsListener : ILoadableService, IUnloadableService {
		// [Dependency]
		// private readonly FloatingTextsSystem _FloatingTextsSystem;
		// [Dependency]
		// private readonly IHeroProvider _HeroProvider;
		
		public void Load() {
			// Unit.Events.onTakesDamage += OnTakeDamage;
			// Unit.Events.onGainsResource += OnGainResource;
		}

		public void Unload() {
			// Unit.Events.onTakesDamage -= OnTakeDamage;
			// Unit.Events.onGainsResource -= OnGainResource;
		}

		// private void OnTakeDamage(Unit unit, Damage damage, Damage.Result result) {
		// 	if(unit == _HeroProvider.Hero)
		// 		return;
		// 	if(damage.source != _HeroProvider.Hero)
		// 		return;
		// 	if(result.damage.value <= 0)
		// 		return;
		// 	var data = FloatingTextDataConfig.Instance.FloatingDmg;
		// 	data.Text = $"-{Mathf.Round(result.damage.value)}";
		// 	var randInSphere = Random.insideUnitSphere * 0.4f;
		// 	data.FromPos = unit.position + new Vector3(randInSphere.x, randInSphere.y + 2, randInSphere.z);
		// 	_FloatingTextsSystem.PlayWorldPos(data);
		// }
		//
		// private void OnGainResource(Unit unit, Type type, float amount) {
		// 	if(unit != _HeroProvider.Hero)
		// 		return;
		// 	if (type == typeof(Game.Unit.Health)) {
		// 		var data = FloatingTextDataConfig.Instance.FloatingHeal;
		// 		data.Text = $"+{Mathf.Round(amount)}";
		// 		var randInSphere = Random.insideUnitSphere * 0.4f;
		// 		data.FromPos = unit.position + new Vector3(randInSphere.x, randInSphere.y + 2, randInSphere.z);
		// 		_FloatingTextsSystem.PlayWorldPos(data);
		// 	}
		// }
	}
}