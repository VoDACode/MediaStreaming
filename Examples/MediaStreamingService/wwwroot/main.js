/*
---------------------------Library------------------------------------
 */

class Client{
    id;
    host;
    room;
    url;
}

class MediaStreaming extends Client{
    voiceStream;
    screenStream;
    notificationStream;

    oncall;
    onstartscreenstream;
    oninit

    constructor(host, mainRoom) {
        super();
        this.room = mainRoom;
        this.host = host;
        let url = `https://${this.host}/logon`
        $.post(url, (data) => {
            this.id = data.Id;
            this.voiceStream = new VoiceStream(this.host, this.room, this.id);
            this.screenStream = new ScreenStream(this.host, this.room, this.id);
            this.notificationStream = new WebSocket(`wss://${this.host}/notification?id=${this.id}`);
            this.notificationStream.onmessage = this.eventNotification;
            if(this.oninit != null)
                this.oninit(this.id);
        });
    }

    setRoom(room){
        let url = `https://${this.host}/set/room?room=${room}&id=${this.id}`;
        $.post(url);
        this.room = room;
    }

    stopAll(){
        this.voiceStream.stop();
        this.screenStream.stop();
        this.notificationStream.close();
    }

    stop(){
        if(this.voiceStream != null)
            this.voiceStream.stop();
        if(this.screenStream != null)
            this.screenStream.stop();
    }

    subscribeRoom(room){
         return $.post(`https://${this.host}/subscribe?room=${room}&id=${this.id}`);
    }

    unsubscribeRoom(room){
        return $.ajax({
            url: `https://${this.host}/unsubscribe?room=${room}&id=${this.id}`,
            async: false,
            method: 'DELETE',
        });
    }

    eventNotification = (event) => {
        console.log(event.data);
        let data = JSON.parse(event.data);

        if(!(!data.data)){
            if(data.data.type === "call") {
                if (this.oncall !== null)
                    this.oncall(data);
                return;
            }else if(data.data.type === "StartScreenStream"){
                if (this.onstartscreenstream !== null) {
                    console.log(this.screenStream.chunks);
                    this.onstartscreenstream(data);

                }
                return;
            }
        }
    }
}

class VoiceStream extends Client{
    wsVoice;
    wsListen;
    isStart = false;
    audioContext = new AudioContext();
    BUFF_SIZE = 1024*8;
    microphone_stream = null;
    gain_node = null;
    script_processor_node = null;
    ChunkArray = [];

    constructor(host, room, id) {
        super();
        this.id = id;
        this.host = host;
        this.room = room;
    }

    async start(){
        if(this.isStart && this.microphone_stream != null){
            this.microphone_stream.connect(this.script_processor_node);
        }
        if(this.isStart)
            return;
        this.isStart = true;
        this.wsVoice = new WebSocket(`wss://${this.host}/voice/start?room=${this.room}&id=${this.id}`);
        this.wsListen = new WebSocket(`wss://${this.host}/voice/listen?room=${this.room}&id=${this.id}`);
        this.wsListen.onmessage = this.OnMessageEvent;

        navigator.mediaDevices.getUserMedia({audio: true}).then(mediaStream => {
            if(this.microphone_stream == null)
                this.start_microphone(mediaStream);
        });
    }

    async stop() {
        if(!this.isStart)
            return;
        this.isStart = false;

        this.microphone_stream.disconnect();
        await this.wsVoice.close();
        await this.wsListen.close();
    }

    OnMessageEvent = async (data) => {
        const arr = new Float32Array(await (new Response(data.data)).arrayBuffer());
        this.ChunkArray.push(arr);
        console.log(arr);
        await this.playChunks();
    }

    playChunks(){
        return new Promise(async () => {
            if (this.ChunkArray.length === 0)
                return;
            let chunk = this.ChunkArray[0];
            this.ChunkArray = this.ChunkArray.slice(1);

            await this.play(chunk);
        });
    }
    play(chunk) {
        return new Promise(() => {
            let audioBuffer = this.audioContext.createBuffer(1, this.BUFF_SIZE, 48000);
            audioBuffer.copyToChannel(chunk, 0);
            let source = this.audioContext.createBufferSource();
            source.buffer = audioBuffer;
            source.connect(this.audioContext.destination);
            source.start();
        });
    }

    start_microphone(stream){
        this.gain_node = this.audioContext.createGain();
        this.gain_node.connect(this.audioContext.destination);

        this.microphone_stream = this.audioContext.createMediaStreamSource(stream);

        this.script_processor_node = this.audioContext.createScriptProcessor(this.BUFF_SIZE, 1, 1);
        this.script_processor_node.onaudioprocess = this.process_microphone_buffer;

        this.microphone_stream.connect(this.script_processor_node);

        // --- enable volume control for output speakers

        this.gain_node.gain.value = 20;
    }

    process_microphone_buffer = (event) => {
        let microphone_output_buffer = event.inputBuffer.getChannelData(0);
        let avg = 0;
        for(let i = 0; i < microphone_output_buffer.length; i++){
            avg += microphone_output_buffer[i];
        }
        avg = avg/microphone_output_buffer.length;
        if(avg <= 1 && avg >= -0.05 && avg !== 0) {
            let arr = new Blob([microphone_output_buffer])
            this.wsVoice.send(arr);
        }
    }
}

class ScreenStream extends Client{
    mySocketStream;
    recorder;
    stream;
    chunks;
    chunksUrl;
    joinStreams = [];

    constructor(host, room, id) {
        super();
        this.chunks = new Blob([], {type: "video/webm; codecs=vp8"});
        this.chunksUrl = URL.createObjectURL(this.chunks);
        this.id = id;
        this.host = host;
        this.room = room;
        this.url = `wss://${this.host}/screen?room=${this.room}&id=${this.id}`;
    }

    async start(){
        this.mySocketStream = new WebSocket(this.url);
        try {
            let params = {video: {cursor: "always" },audio: {echoCancellation: true, noiseSuppression: true, sampleRate: 44100}};
            this.stream = await navigator.mediaDevices.getDisplayMedia(params);
            console.log(this.stream);
        } catch(err) {
            console.error("Error: " + err);
        }
        this.recorder = new MediaRecorder(this.stream);
        this.recorder.ondataavailable = (event) => {
            if(event.data.size === 0)
                return;
            this.mySocketStream.send(event.data)
        }
        this.recorder.start(30);
    }

    joinToSteam(streamId){
        let url = `wss://${this.host}/stream-view?room=${this.room}&id=${this.id}&streamId=${streamId}`;
        let stream = new WebSocket(url);
        stream.onmessage = this.responceStreamData
        this.joinStreams.push(stream);
    }

    stop(){
        if(this.recorder !== undefined)
            this.recorder.stop();
    }

    responceStreamData = (e) => {
        this.chunks = e.data.slice(0, e.data.size, "video/webm");
        //this.chunks = new Blob(e.data.arrayBuffer(), {type: "video/webm; codecs=vp8"});
        //this.chunks.push(e.data);
    }

    OnMessageWebSocket = (e) => {
        Array.prototype.push.apply(this.chunks, e.data);
        console.log(e);
        console.log(this.chunks);
        //document.getElementById("video1").srcObject = e.data;
    }
}

/*
------------------------Example-------------------------------
 */

let Room = "main";

let subRooms = [];

let HOST = location.host;

function saveHost(){
    HOST = $('#host').val() + ':' + $('#port').val();
    $('#connectUrl').val(HOST);
}

let mediaStreaming = new MediaStreaming(`${HOST}/ws`, Room);

$('#host').val(location.hostname)
$('#port').val(location.port)
saveHost();

$("#subButton").click(function (){
    let room = $("#subRoomName").val();
    if(subRooms.find(p => p === room) != undefined)
        return;
    subRooms.push(room);
    $("#subRoomsList").append(new Option(room, room));
    mediaStreaming.subscribeRoom(room);
});

$("#setRoomButton").click(() =>{
    mediaStreaming.setRoom($("#roomName").val());
});

$('#roomName').val(Room);

mediaStreaming.oncall = (data) => {
    console.log(data);
}

mediaStreaming.onstartscreenstream = (data) => {
    let video = $('<video/>', {
        type: 'video/webm; codecs=vp8',
        controls: false,
        className: "stream-player",
        id: `stream-${data.data.stream.id}`,
        src: mediaStreaming.screenStream.chunksUrl
    });
    video.appendTo("#videoBox");
};

mediaStreaming.oninit = (id) => {
    mediaStreaming.setRoom(Room);
    console.log(id);
}

window.onbeforeunload = (e) => {
    mediaStreaming.stop();
}
