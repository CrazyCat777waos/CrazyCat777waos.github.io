using Microsoft.JSInterop;

namespace AppWebTareas.Services
{
    public class WebSoundService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _soundsEnabled = true;

        public WebSoundService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> PlayScratchSound()
        {
            return await PlaySound("playScratchSound");
        }

        public async Task<bool> PlayAddSound()
        {
            return await PlaySound("playAddSound");
        }

        public async Task<bool> PlayDeleteSound()
        {
            return await PlaySound("playDeleteSound");
        }

        public async Task<bool> PlayPaperSound()
        {
            return await PlaySound("playPaperSound");
        }

        private async Task<bool> PlaySound(string functionName)
        {
            if (!_soundsEnabled) return false;

            try
            {
                return await _jsRuntime.InvokeAsync<bool>(functionName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
                return false;
            }
        }

        public void ToggleSounds(bool enabled)
        {
            _soundsEnabled = enabled;
        }

        public bool AreSoundsEnabled() => _soundsEnabled;
    }
}