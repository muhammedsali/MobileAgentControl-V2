package com.example.mobileagentcontrol

import android.content.Context
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.BaseAdapter
import android.widget.ImageView
import android.widget.TextView

class CharacterAdapter(
    private val context: Context,
    private val characters: List<Character>
) : BaseAdapter() {

    override fun getCount(): Int = characters.size

    override fun getItem(position: Int): Any = characters[position]

    override fun getItemId(position: Int): Long = position.toLong()

    override fun getView(position: Int, convertView: View?, parent: ViewGroup?): View {
        val view = convertView ?: LayoutInflater.from(context)
            .inflate(R.layout.item_character, parent, false)

        val character = characters[position]
        
        view.findViewById<ImageView>(R.id.characterImage)
            .setImageResource(character.imageResource)
        
        view.findViewById<TextView>(R.id.characterName)
            .text = character.name

        return view
    }
} 