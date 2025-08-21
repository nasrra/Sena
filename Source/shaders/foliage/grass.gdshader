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
const int MAX_DEFORMERS = 32;
uniform int deformer_count = 0;
uniform vec3 deformers_position[MAX_DEFORMERS];
uniform float deformers_radius[MAX_DEFORMERS];
uniform float deformers_strength[MAX_DEFORMERS];
uniform sampler2D character_distance_falloff_curve;

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

	for(int i = 0; i < MAX_DEFORMERS; i++){
		if(i >= deformer_count){
			break;
		}

		vec3 dir_to_deformer = deformers_position[i] - (MODEL_MATRIX[3]).xyz;
	    // Uncomment for horizontal-only displacement
	    // dir_to_character.y = 0.0;

	    float distance_to_deformer = length(dir_to_deformer);
		float deformer_radius = deformers_radius[i];
		if (distance_to_deformer < deformer_radius) {
			float norm_distance = clamp(distance_to_deformer / deformer_radius, 0.0, 1.0);
			if (norm_distance < 0.99) {
			    dir_to_deformer = (inverse(MODEL_MATRIX) * vec4(dir_to_deformer, 0.0)).xyz;
			    dir_to_deformer = normalize(dir_to_deformer);
			    float falloff_curve = clamp(texture(character_distance_falloff_curve, vec2(norm_distance, 0.0)).r, 0.0, 1.0);
			    VERTEX += normalize(-dir_to_deformer) * falloff_curve * deformers_strength[i] * displacement_affect;
			}
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
