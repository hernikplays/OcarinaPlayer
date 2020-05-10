package cz.hernikplays.Ocarina;

import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.scene.control.Alert;
import javafx.scene.control.ButtonType;
import javafx.scene.media.Media;
import javafx.scene.media.MediaPlayer;
import javafx.stage.FileChooser;
import javafx.stage.Stage;

import java.io.File;

public class Controller {
    private Stage myStage;
    MediaPlayer mediaPlayer;
    FileChooser playFile = new FileChooser();
    File selected;

    public void initialize(){

    }

    public void setStage(Stage stage) {
        myStage = stage;
    }

    public void openFile(ActionEvent event){
        selected = playFile.showOpenDialog(myStage);
    }

    @FXML
    public void play(ActionEvent event){
        Alert nofile = new Alert(Alert.AlertType.ERROR, "No file selected", ButtonType.OK);
        if(selected == null){
            nofile.show();
            return;
        }
        Media hit = new Media(selected.toURI().toString());
        mediaPlayer = new MediaPlayer(hit);
        mediaPlayer.setCycleCount(MediaPlayer.INDEFINITE);
        mediaPlayer.play();

        /*MediaView mediaView = new MediaView(mediaPlayer);
        ((Group)scene.getRoot()).getChildren().add(mediaView);*/
    }
}
