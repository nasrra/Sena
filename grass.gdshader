shader_type spatial;

render_mode depth_draw_opaque, cull_disabled;

// Texture settings
uniform sampler2D texture_albedo : hint_default_white, repeat_disable;
//uniform sampler2D texture_gradient : hint_default_white, repeat_disable;
uniform sampler2D texture_noise : hint_default_white;
uniform float alpha_scissor_threshold : hint_range(0.0, 1.0);
uniform vec4 transmission : source_color;
uniform vec4 secondary_color : source_color;
uniform float secondary_attenuation = 0.2;
uniform float grass_height = 1.0;

// Wind settings
uniform vec2 wind_direction = vec2(1, -0.5);
uniform float wind_speed = 1.0;
uniform float wind_strength = 2.0;
uniform float noise_scale = 20.0;

// Character Deformation:
uniform vec3 character_position;
uniform float character_radius = 3.0;
uniform sampler2D character_distance_falloff_curve;
uniform float character_push_strength = 1.0;

instance uniform float camera_bend_strength : hint_range(0.0, 3.0) = 0.2;

varying float color;
varying float height;

void vertex() {
	height = VERTEX.y;
	float influence = smoothstep(0, 1, height / 2.0);
	vec4 world_pos = MODEL_MATRIX * vec4(VERTEX, 1.0);
	vec2 uv = world_pos.xz / (noise_scale + 1e-2);
	vec2 panning_uv = uv + fract(TIME * wind_direction * wind_speed);
	float wind = texture(texture_noise, panning_uv).r * 2.0 - 0.4;
	color = texture(texture_noise, uv).r;

	vec2 wind_offset = -wind_direction * wind_strength * influence * wind;
	world_pos.xz += wind_offset;
	world_pos.y -= wind * influence * smoothstep(0.0, height, wind_strength);

	//Push the top vertex away from the camera to bend the grass clump
	float ndotv = 1.0 - dot(vec3(0.0, 1.0, 0.0), normalize(INV_VIEW_MATRIX[1].xyz));
	world_pos.xz += INV_VIEW_MATRIX[1].xz * camera_bend_strength * height * ndotv;

	vec4 local_pos = inverse(MODEL_MATRIX) * world_pos;
	local_pos.x += wind_strength * influence * cos(TIME * 1.0) / 8.0;
	local_pos.z += wind_strength * influence * sin(TIME * 1.5) / 8.0;

	VERTEX = local_pos.xyz;
	//NORMAL = vec3(0.0, 1.0, 0.0);

	float displacement_affect = (1.0 - UV.y);

	vec3 dir_to_character = character_position - (MODEL_MATRIX[3]).xyz;
    // Uncomment for horizontal-only displacement
    // dir_to_character.y = 0.0;

    float distance_to_character = length(dir_to_character);
	if (distance_to_character < character_radius) {
		float norm_distance = clamp(distance_to_character / character_radius, 0.0, 1.0);
		if (norm_distance < 0.99) {
		    dir_to_character = (inverse(MODEL_MATRIX) * vec4(dir_to_character, 0.0)).xyz;
		    dir_to_character = normalize(dir_to_character);
		    float falloff_curve = clamp(texture(character_distance_falloff_curve, vec2(norm_distance, 0.0)).r, 0.0, 1.0);
		    VERTEX += normalize(-dir_to_character) * falloff_curve * character_push_strength * displacement_affect;
		}
	}

}

void fragment() {
    vec4 tex = texture(texture_albedo, UV);
    if (tex.a < alpha_scissor_threshold) {
        discard;
    }
    ALBEDO = tex.rgb;
}

//void fragment() {
	//vec4 tex = texture(texture_albedo, UV);
	//if (tex.a < alpha_scissor_threshold) {
	//	discard;
	//}

	//BACKLIGHT = transmission.rgb;
	//vec4 gradient = texture(texture_gradient, vec2(height / grass_height, 0.0));
	//float secondary_weight = smoothstep(0.0, 1.0, color - secondary_attenuation);
	//ALBEDO = tex.rbg * gradient.rgb;
	//ALBEDO = mix(ALBEDO, secondary_color.rgb, secondary_weight);
//}
