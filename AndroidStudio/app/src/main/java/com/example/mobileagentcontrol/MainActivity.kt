package com.example.mobileagentcontrol

import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.util.Log
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.viewpager2.widget.ViewPager2
import com.google.android.material.tabs.TabLayout
import com.google.android.material.tabs.TabLayoutMediator
import com.google.firebase.firestore.FirebaseFirestore

class MainActivity : AppCompatActivity() {
    private lateinit var viewPager: ViewPager2
    private lateinit var tabLayout: TabLayout
    private lateinit var db: FirebaseFirestore
    private val TAG = "MainActivity"
    
    private val roles = listOf(
        "Tümü",
        "Düellocu",
        "Öncü",
        "Gözcü",
        "Kontrol Uzmanı"
    )
    
    private val charactersByRole = mapOf(
        "Düellocu" to listOf(
            Character("Jett", R.drawable.agent_jett, 622, 475),
            Character("Raze", R.drawable.agent_raze, 958, 454),
            Character("Reyna", R.drawable.agent_reyna, 1260, 445),
            Character("Yoru", R.drawable.agent_yoru, 622, 475),
            Character("Neon", R.drawable.agent_neon, 958, 454),
            Character("Phoenix", R.drawable.agent_phoenix, 1260, 445),
            Character("Iso", R.drawable.agent_iso, 958, 454),
            Character("Clove", R.drawable.agent_clove, 958, 454)
        ),
        "Öncü" to listOf(
            Character("Breach", R.drawable.agent_breach, 622, 475),
            Character("Sova", R.drawable.agent_sova, 622, 475),
            Character("Fade", R.drawable.agent_fade, 622, 475),
            Character("Skye", R.drawable.agent_skye, 1260, 445),
            Character("Kay/o", R.drawable.agent_kayo, 622, 475),
            Character("Gekko", R.drawable.agent_gekko, 1260, 445),
            Character("Vyse", R.drawable.agent_vyse, 1260, 445)
        ),
        "Gözcü" to listOf(
            Character("Killjoy", R.drawable.agent_killjoy, 958, 454),
            Character("Cypher", R.drawable.agent_cypher, 1260, 445),
            Character("Sage", R.drawable.agent_sage, 958, 454),
            Character("Chamber", R.drawable.agent_chamber, 958, 454),
            Character("Deadlock", R.drawable.agent_deadlock, 622, 475)
        ),
        "Kontrol Uzmanı" to listOf(
            Character("Omen", R.drawable.agent_omen, 1260, 445),
            Character("Brimstone", R.drawable.agent_brimstone, 622, 475),
            Character("Viper", R.drawable.agent_viper, 958, 454),
            Character("Astra", R.drawable.agent_astra, 958, 454),
            Character("Harbor", R.drawable.agent_harbor, 958, 454),
            Character("Tejo", R.drawable.agent_tejo, 958, 454)
        )
    )

    private val allCharacters: List<Character> by lazy {
        charactersByRole.values.flatten()
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // Firebase başlatma
        db = FirebaseFirestore.getInstance()
        Log.d(TAG, "Firebase başlatıldı")

        setupViewPager()
        setupTabLayout()
    }

    private fun setupViewPager() {
        viewPager = findViewById(R.id.viewPager)
        val pagerAdapter = CharacterPagerAdapter(this, roles, charactersByRole, allCharacters) { character ->
            handleCharacterSelection(character)
        }
        viewPager.adapter = pagerAdapter
    }

    private fun setupTabLayout() {
        tabLayout = findViewById(R.id.tabLayout)
        
        TabLayoutMediator(tabLayout, viewPager) { tab, position ->
            tab.text = roles[position]
        }.attach()
    }

    private fun handleCharacterSelection(character: Character) {
        Log.d(TAG, "${character.name} karakteri seçildi. X: ${character.x}, Y: ${character.y}")
        sendCommand("select", character.x, character.y)
        Toast.makeText(this, "${character.name} seçildi", Toast.LENGTH_SHORT).show()
        
        // 1 saniye sonra kilitle
        Handler(Looper.getMainLooper()).postDelayed({
            Log.d(TAG, "Kilitleme komutu gönderiliyor...")
            sendCommand("lock", 950, 867)
            Toast.makeText(this, "Karakter kilitlendi", Toast.LENGTH_SHORT).show()
        }, 1000)
    }

    private fun sendCommand(action: String, x: Int, y: Int) {
        val command = hashMapOf(
            "action" to action,
            "x" to x.toLong(),
            "y" to y.toLong(),
            "timestamp" to System.currentTimeMillis()
        )

        Log.d(TAG, "Komut gönderiliyor: action=$action, x=$x, y=$y")

        db.collection("commands")
            .add(command)
            .addOnSuccessListener {
                Log.d(TAG, "Komut başarıyla gönderildi: $action, döküman ID: ${it.id}")
            }
            .addOnFailureListener { e ->
                Log.e(TAG, "Komut gönderme hatası: ${e.message}")
                Toast.makeText(this, "Hata: ${e.message}", Toast.LENGTH_SHORT).show()
            }
    }
}