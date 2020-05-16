package cz.hernikplays.Ocarina;

import com.mpatric.mp3agic.ID3v2;
import com.mpatric.mp3agic.InvalidDataException;
import com.mpatric.mp3agic.Mp3File;
import com.mpatric.mp3agic.UnsupportedTagException;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.Alert;
import javafx.scene.control.ButtonType;
import javafx.scene.control.Slider;
import javafx.scene.media.Media;
import javafx.scene.media.MediaPlayer;
import javafx.stage.FileChooser;
import javafx.stage.Stage;
import net.arikia.dev.drpc.DiscordEventHandlers;
import net.arikia.dev.drpc.DiscordRPC;
import net.arikia.dev.drpc.DiscordRichPresence;



import java.io.File;
import java.io.IOException;
import java.util.List;

public class Controller {
    public Slider volumeSlider;
    double volumeButSongIsNotPlaying;
    private Stage myStage;
    MediaPlayer mediaPlayer;
    FileChooser playFile = new FileChooser();
    FileChooser.ExtensionFilter extFilter = new FileChooser.ExtensionFilter("Music files (*.mp3)","*.mp3");
    int fileIndex = 0;

    List<File> selected;

    DiscordEventHandlers handlers;

    public void initialize(){
        DiscordRPC.discordInitialize("690238946378121296", handlers, true);
        DiscordRichPresence rich = new DiscordRichPresence.Builder("...").setDetails("Not listening to anything.").setBigImage("rpcon", "Ocarina Music Player").build();
        DiscordRPC.discordUpdatePresence(rich);
        FileChooser.ExtensionFilter extFilter = new FileChooser.ExtensionFilter("Music files (*.mp3)","*.mp3");
        playFile.getExtensionFilters().add(extFilter);
    }

    public void setStage(Stage stage) {
        myStage = stage;
    }

    public void openFile(ActionEvent event){
        selected = playFile.showOpenMultipleDialog(myStage);
    }

    @FXML
    public void play(ActionEvent event) throws InvalidDataException, IOException, UnsupportedTagException {

        Alert nofile = new Alert(Alert.AlertType.ERROR, "No file selected", ButtonType.OK);
        if(selected == null){
            nofile.show();
            return;
        }
        if(mediaPlayer == null){
            System.out.println("mediaPlayer null");
        }
        else if(mediaPlayer.getStatus() == MediaPlayer.Status.PLAYING){
            mediaPlayer.pause();
            System.out.println("Pausing...");
            return;
        }
        else if(mediaPlayer.getStatus() == MediaPlayer.Status.PAUSED){
            mediaPlayer.play();
            System.out.println("Resuming...");
            return;
        }

        Media hit = new Media(selected.get(fileIndex).toURI().toString());
        mediaPlayer = new MediaPlayer(hit);
        mediaPlayer.setCycleCount(MediaPlayer.INDEFINITE);
        mediaPlayer.setOnEndOfMedia(() -> {
            mediaPlayer.stop();
            try {
                skip(event);
            } catch (InvalidDataException | IOException | UnsupportedTagException e) {
                e.printStackTrace();
            }
        });
        mediaPlayer.play();

        Mp3File nowPlaying = new Mp3File(selected.get(fileIndex));
        if(nowPlaying.hasId3v2Tag()){
            ID3v2 tag = nowPlaying.getId3v2Tag();
            String track = tag.getTitle();
            String artist = tag.getArtist();

            DiscordRPC.discordUpdatePresence(new DiscordRichPresence.Builder("by "+artist).setDetails("Listening to "+track).setBigImage("rpcon", "Ocarina Music Player").build());
        }
        /*MediaView mediaView = new MediaView(mediaPlayer);
        ((Group)scene.getRoot()).getChildren().add(mediaView);*/
    }

    public void skip (ActionEvent event) throws InvalidDataException, IOException, UnsupportedTagException {
        if(selected == null){
            System.out.println("Selected is null");
            return;
        }
        Alert eop = new Alert(Alert.AlertType.ERROR, "End of playlist", ButtonType.OK);
        fileIndex++;
        if(fileIndex == selected.size()){
         fileIndex=0;
         return;
        }
        if(mediaPlayer == null){
            System.out.println("mediaPlayer null");
        }
        else if(mediaPlayer.getStatus() == MediaPlayer.Status.PLAYING){
            mediaPlayer.stop();
        }

        play(event);
    }

    public void prev (ActionEvent event) throws InvalidDataException, IOException, UnsupportedTagException {
        if(selected == null){
            System.out.println("Selected is null");
            return;
        }

        System.out.println(fileIndex);
        if(fileIndex == 0){
            fileIndex=selected.size()-1;
            return;
        }
        fileIndex--;
        if(mediaPlayer == null){
            System.out.println("mediaPlayer null");
        }
        else if(mediaPlayer.getStatus() == MediaPlayer.Status.PLAYING){
            mediaPlayer.stop();
        }
        play(event);
    }
    @FXML
    public void changeVolume(){
        System.out.println(volumeSlider.getValue());
        if(mediaPlayer == null || mediaPlayer.getStatus() == MediaPlayer.Status.STOPPED){
            volumeButSongIsNotPlaying = volumeSlider.getValue();
        }
        else {
            volumeButSongIsNotPlaying = 0;
            double value = volumeSlider.getValue();
            mediaPlayer.setVolume(value);
        }
    }
}
