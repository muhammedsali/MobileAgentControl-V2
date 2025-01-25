package com.example.mobileagentcontrol

import android.content.Context
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.GridView
import androidx.recyclerview.widget.RecyclerView

class CharacterPagerAdapter(
    private val context: Context,
    private val roles: List<String>,
    private val charactersByRole: Map<String, List<Character>>,
    private val allCharacters: List<Character>,
    private val onCharacterSelected: (Character) -> Unit
) : RecyclerView.Adapter<CharacterPagerAdapter.CharacterPageViewHolder>() {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): CharacterPageViewHolder {
        val gridView = GridView(context).apply {
            layoutParams = ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
            )
            numColumns = 3
            horizontalSpacing = context.resources.getDimensionPixelSize(R.dimen.grid_spacing)
            verticalSpacing = context.resources.getDimensionPixelSize(R.dimen.grid_spacing)
            clipToPadding = false
        }
        return CharacterPageViewHolder(gridView)
    }

    override fun onBindViewHolder(holder: CharacterPageViewHolder, position: Int) {
        val characters = when (position) {
            0 -> allCharacters
            else -> charactersByRole[roles[position]] ?: emptyList()
        }
        
        val adapter = CharacterAdapter(context, characters)
        holder.gridView.adapter = adapter
        holder.gridView.setOnItemClickListener { _, _, pos, _ ->
            onCharacterSelected(characters[pos])
        }
    }

    override fun getItemCount(): Int = roles.size

    class CharacterPageViewHolder(val gridView: GridView) : RecyclerView.ViewHolder(gridView)
} 