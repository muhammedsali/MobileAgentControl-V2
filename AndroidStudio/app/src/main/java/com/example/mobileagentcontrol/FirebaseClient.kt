package com.example.mobileagentcontrol

import com.google.firebase.firestore.FirebaseFirestore
import kotlinx.coroutines.tasks.await

class FirebaseClient {
    private val db = FirebaseFirestore.getInstance()
    private val commandsCollection = db.collection("commands")

    suspend fun sendCharacterSelect(character: Character) {
        val command = hashMapOf(
            "action" to "select",
            "x" to character.x,
            "y" to character.y,
            "timestamp" to System.currentTimeMillis()
        )
        commandsCollection.add(command).await()
    }

    suspend fun sendLockCommand(x: Int, y: Int) {
        val command = hashMapOf(
            "action" to "lock",
            "x" to x,
            "y" to y,
            "timestamp" to System.currentTimeMillis()
        )
        commandsCollection.add(command).await()
    }
} 