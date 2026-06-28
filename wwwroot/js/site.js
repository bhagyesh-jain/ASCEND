// ==========================================
// ASCEND CORE CLIENT ENGINE
// ==========================================

// Sound synthesis settings
let soundEnabled = true;

// Initialize Sound Toggle from LocalStorage
if (localStorage.getItem('soundEnabled') === 'false') {
  soundEnabled = false;
}

// Web Audio API Synthesizer
const AudioSynth = {
  ctx: null,

  init() {
    if (!this.ctx) {
      this.ctx = new (window.AudioContext || window.webkitAudioContext)();
    }
  },

  playTone(freq, type, duration, startTime = 0) {
    if (!soundEnabled) return;
    this.init();

    const osc = this.ctx.createOscillator();
    const gainNode = this.ctx.createGain();

    osc.type = type;
    osc.frequency.setValueAtTime(freq, this.ctx.currentTime + startTime);
    
    gainNode.gain.setValueAtTime(0.1, this.ctx.currentTime + startTime);
    gainNode.gain.exponentialRampToValueAtTime(0.0001, this.ctx.currentTime + startTime + duration);

    osc.connect(gainNode);
    gainNode.connect(this.ctx.destination);

    osc.start(this.ctx.currentTime + startTime);
    osc.stop(this.ctx.currentTime + startTime + duration);
  },

  playQuestComplete() {
    // Rising arpeggio: C5 -> E5 -> G5 -> C6
    const notes = [523.25, 659.25, 783.99, 1046.50];
    notes.forEach((freq, index) => {
      this.playTone(freq, 'triangle', 0.35, index * 0.08);
    });
  },

  playLevelUp() {
    // Triumphant fanfare
    const melody = [
      { f: 261.63, d: 0.15 }, // C4
      { f: 329.63, d: 0.15 }, // E4
      { f: 392.00, d: 0.15 }, // G4
      { f: 523.25, d: 0.3 },  // C5
      { f: 392.00, d: 0.15 }, // G4
      { f: 523.25, d: 0.6 }   // C5 (triumphant hold)
    ];

    let time = 0;
    melody.forEach((note) => {
      this.playTone(note.f, 'sawtooth', note.d, time);
      // Play a parallel harmony note (E5/G5) to make it richer
      this.playTone(note.f * 1.5, 'sine', note.d, time);
      time += note.d - 0.02;
    });
  }
};

// Toast Notifications
function showToast(message, icon = 'fa-trophy') {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const toast = document.createElement('div');
  toast.className = 'toast-message';
  toast.innerHTML = `<i class="fas ${icon}"></i><span>${message}</span>`;

  container.appendChild(toast);

  // Remove toast after 4 seconds
  setTimeout(() => {
    toast.style.animation = 'slideToastIn 0.3s reverse forwards';
    setTimeout(() => {
      toast.remove();
    }, 300);
  }, 4000);
}

// Level Up Modal Controller
function triggerLevelUp(newLevel, newRank) {
  AudioSynth.playLevelUp();

  const modal = document.getElementById('level-up-modal');
  const lvlBadge = document.getElementById('modal-level-badge');
  const rankText = document.getElementById('modal-rank-text');

  if (modal && lvlBadge) {
    lvlBadge.innerText = newLevel;
    if (rankText) rankText.innerText = newRank;
    modal.classList.add('active');
  }

  // Update DOM elements on page
  const levelDisplays = document.querySelectorAll('.user-level-value');
  levelDisplays.forEach(el => el.innerText = newLevel);

  const rankDisplays = document.querySelectorAll('.rank-badge, .user-rank-value');
  rankDisplays.forEach(el => el.innerText = newRank);
}

// Close level up modal
function closeLevelUpModal() {
  const modal = document.getElementById('level-up-modal');
  if (modal) {
    modal.classList.remove('active');
  }
}

// Document Ready
document.addEventListener('DOMContentLoaded', () => {
  // Setup Sound Toggle on Profile page if exists
  const soundToggle = document.getElementById('sound-settings-toggle');
  if (soundToggle) {
    soundToggle.checked = soundEnabled;
    soundToggle.addEventListener('change', (e) => {
      soundEnabled = e.target.checked;
      localStorage.setItem('soundEnabled', soundEnabled);
      showToast(soundEnabled ? 'Sound effects enabled!' : 'Sound effects muted.', 'fa-volume-up');
      if (soundEnabled) AudioSynth.playQuestComplete();
    });
  }

  // Bind Quest Checkboxes (AJAX)
  const questCheckboxes = document.querySelectorAll('.quest-checkbox');
  questCheckboxes.forEach(checkbox => {
    checkbox.addEventListener('change', async function() {
      const habitId = this.dataset.habitId;
      const questItem = this.closest('.quest-item');

      if (this.checked) {
        // Complete Quest
        try {
          const response = await fetch(`/Home/CompleteQuest?id=${habitId}`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
          });
          const data = await response.json();

          if (data.success) {
            questItem.classList.add('completed');
            AudioSynth.playQuestComplete();

            const gamification = data.gamification;
            
            // Update XP Bar
            const xpBar = document.querySelector('.xp-bar-fill');
            const xpText = document.querySelector('.xp-text-value');
            if (xpBar && xpText) {
              const nextThreshold = gamification.newXP + (gamification.xpNeededForNextLevel || 100); // Wait, we returned a custom structure
              // Let's look at what we sent in GamificationResult:
              // NewXP, NewLevel, LevelUp, NewRank, UnlockedAchievements, CurrentStreak
              // Let's write the bar update:
              const xpNeeded = gamification.xpNeededForNextLevel || parseInt(xpBar.dataset.nextThreshold || 100);
              // Wait, let's look at HomeController's CompleteQuest action:
              // It returns `Json(new { success = true, gamification = result })`
              // Let's calculate the percentage:
              // Wait, let's fetch the next level threshold on level up or dynamically
              // If LevelUp, we can trigger the modal
            }

            // Since we might need to refresh the page to update all statistics and complex UI items
            // but we want a smooth transition, we can do a soft-update or reload after a short delay,
            // or update the numbers dynamically. Let's update the numbers dynamically!
            if (gamification.levelUp) {
              triggerLevelUp(gamification.newLevel, disappointedRankOverride(gamification.newRank));
            } else {
              showToast(`Quest Completed! +${gamification.xpEarned} XP`, 'fa-check-circle');
            }

            // Show toasts for achievements
            if (gamification.unlockedAchievements && gamification.unlockedAchievements.length > 0) {
              gamification.unlockedAchievements.forEach(ach => {
                setTimeout(() => {
                  showToast(`ACHIEVEMENT UNLOCKED: ${ach}!`, 'fa-trophy');
                }, 800);
              });
            }

            // Update streak on UI
            const streakVal = document.querySelector('.streak-value');
            if (streakVal) streakVal.innerText = gamification.currentStreak;

            // Wait, let's update the XP progress bar and numbers
            updateXPBarUI(gamification.newXP, gamification.newLevel);

          } else {
            this.checked = false;
            showToast(data.message || 'Error completing quest.', 'fa-exclamation-triangle');
          }
        } catch (err) {
          this.checked = false;
          showToast('Failed to connect to the server.', 'fa-exclamation-triangle');
        }
      } else {
        // Undo Quest Completion
        try {
          const response = await fetch(`/Home/UndoQuest?id=${habitId}`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
          });
          const data = await response.json();

          if (data.success) {
            questItem.classList.remove('completed');
            showToast('Quest completion undone.', 'fa-undo');

            // Update XP and Streak
            const streakVal = document.querySelector('.streak-value');
            if (streakVal) streakVal.innerText = data.streak;

            updateXPBarUI(data.xp, data.level);
          } else {
            this.checked = true;
            showToast(data.message || 'Error undoing quest.', 'fa-exclamation-triangle');
          }
        } catch (err) {
          this.checked = true;
          showToast('Failed to connect to the server.', 'fa-exclamation-triangle');
        }
      }
    });
  });

  function disappointedRankOverride(rank) {
    return rank || "E Rank";
  }

  function updateXPBarUI(currentXP, level) {
    const xpBar = document.querySelector('.xp-bar-fill');
    const xpText = document.querySelector('.xp-text-value');
    if (xpBar && xpText) {
      // Calculate threshold dynamically
      const threshold = getXPThresholdJS(level);
      const percentage = Math.min((currentXP / threshold) * 100, 100);
      xpBar.style.width = `${percentage}%`;
      xpText.innerText = `${currentXP} / ${threshold} XP`;
    }
  }

  function getXPThresholdJS(level) {
    if (level <= 1) return 100;
    if (level === 2) return 250;
    if (level === 3) return 500;
    
    let threshold = 500;
    let diff = 350;
    for (let i = 4; i <= level; i++) {
      threshold += diff;
      diff += 100;
    }
    return threshold;
  }

  // Theme Selector Click Binding (Profile Page)
  const themeCards = document.querySelectorAll('.theme-card');
  themeCards.forEach(card => {
    card.addEventListener('click', async function() {
      const selectedTheme = this.dataset.themeName;

      try {
        const response = await fetch('/Profile/UpdateTheme', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
          },
          body: JSON.stringify({ theme: selectedTheme })
        });
        const data = await response.json();

        if (data.success) {
          // Switch theme in UI immediately
          document.body.setAttribute('data-theme', selectedTheme);
          
          // Update active card state
          themeCards.forEach(c => c.classList.remove('active'));
          this.classList.add('active');

          showToast(`World shifted to [${selectedTheme}]!`, 'fa-globe');
        } else {
          showToast('Failed to save theme setting.', 'fa-exclamation-triangle');
        }
      } catch (err) {
        showToast('Error communicating with server.', 'fa-exclamation-triangle');
      }
    });
  });
});
