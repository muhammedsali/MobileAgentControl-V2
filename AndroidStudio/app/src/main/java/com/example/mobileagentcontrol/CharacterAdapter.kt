package com.example.mobileagentcontrol

import android.content.Context
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.BaseAdapter
import android.widget.ImageView
import android.widget.TextView
import com.bumptech.glide.Glide
import com.bumptech.glide.load.resource.bitmap.CenterCrop
import com.bumptech.glide.load.resource.bitmap.RoundedCorners

class CharacterAdapter(
    private val context: Context,
    private val characters: List<Character>
) : BaseAdapter() {

    override fun getCount(): Int = characters.size

    override fun getItem(position: Int): Character = characters[position]

    override fun getItemId(position: Int): Long = position.toLong()

    override fun getView(position: Int, convertView: View?, parent: ViewGroup?): View {
        val view = convertView ?: LayoutInflater.from(context)
            .inflate(R.layout.item_character, parent, false)

        val character = getItem(position)
        val imageView = view.findViewById<ImageView>(R.id.characterImage)
        val nameView = view.findViewById<TextView>(R.id.characterName)

        // Resmi yükle
        Glide.with(context)
            .load(character.imageResId)
            .transform(CenterCrop(), RoundedCorners(8))
            .into(imageView)

        nameView.text = character.name

        return view
    }
} 