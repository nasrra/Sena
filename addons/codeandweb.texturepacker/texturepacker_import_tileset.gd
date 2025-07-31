# The MIT License (MIT)
#
# Copyright (c) 2018 George Marques
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

@tool
extends EditorImportPlugin

var imageLoader = preload("image_loader.gd").new()

enum Preset { PRESET_DEFAULT }

func _notification(what):
	if what == NOTIFICATION_PREDELETE:
		imageLoader.free()

func get_importer_name():
	return "codeandweb.texturepacker_import_tileset"

func get_visible_name():
	return "TileSet from TexturePacker"

func get_recognized_extensions():
	return ["tpset"]

func get_save_extension():
	return "res"

func get_resource_type():
	return "Resource"

func get_preset_count():
	return Preset.size()

func get_preset_name(preset):
	match preset:
		Preset.PRESET_DEFAULT: return "Default"

func get_import_options(preset):
	return []

func get_option_visibility(option, options):
	return true

func get_import_order():
	return 200

func import(source_file, save_path, options, r_platform_variants, r_gen_files):
	var sheets = read_sprite_sheet(source_file)
	var sheet_folder = source_file.get_basename() + ".sprites"
	create_folder(sheet_folder)

	var file_name = "%s.%s" % [source_file.get_basename(), "res"]

	var tile_set: TileSet
	if FileAccess.file_exists(file_name):
		tile_set = ResourceLoader.load(file_name, "TileSet")
	else:
		tile_set = TileSet.new()

	var used_ids = []
	for sheet in sheets.textures:
		var sheet_file = source_file.get_base_dir() + "/" + sheet.image
		var image = load_image(sheet_file, "ImageTexture", [])
		r_gen_files.push_back(sheet.image)
		create_tiles(tile_set, sheet, image, used_ids)

	prune_tileset(tile_set, used_ids)

	r_gen_files.push_back(file_name)
	ResourceSaver.save(tile_set, file_name)

	return ResourceSaver.save(Resource.new(), "%s.%s" % [save_path, get_save_extension()])

func prune_tileset(tile_set: TileSet, used_ids):
	used_ids.sort()
	for id in tile_set.get_tiles_ids():
		if not used_ids.has(id):
			tile_set.remove_tile(id)

func create_folder(folder):
	var dir = DirAccess.open("res://")
	if not dir.dir_exists(folder):
		if dir.make_dir_recursive(folder) != OK:
			printerr("Failed to create folder: " + folder)

func create_tiles(tile_set, sheet, image, r_used_ids):
	for sprite in sheet.sprites:
		r_used_ids.push_back(create_tile(tile_set, sprite, image))

func create_tile(tile_set, sprite, image):
	var tile_name = sprite.filename.get_basename()

	var id = tile_set.find_tile_by_name(tile_name)
	if id == -1:
		id = tile_set.get_last_unused_tile_id()
		tile_set.create_tile(id)
		tile_set.set_tile_name(id, tile_name)

	tile_set.set_texture(id, image)
	tile_set.set_region(id, Rect2(sprite.region.x, sprite.region.y, sprite.region.w, sprite.region.h))
	tile_set.set_texture_origin(id, Vector2(sprite.margin.x, sprite.margin.y))
	return id

func save_resource(name, texture):
	create_folder(name.get_base_dir())

	var status = ResourceSaver.save(texture, name)
	if status != OK:
		printerr("Failed to save resource " + name)
		return false
	return true

func read_sprite_sheet(file_name):
	var file = FileAccess.open(file_name, FileAccess.READ)
	if file == null:
		printerr("Failed to load " + file_name)
		return {}

	var text = file.get_as_text()
	file.close()

	var json = JSON.new()
	var result = json.parse(text)
	if result != OK:
		printerr("Invalid JSON data in " + file_name)
		return {}

	return json.data

func load_image(rel_path, source_path, options):
	return imageLoader.load_image(rel_path, source_path, options)
